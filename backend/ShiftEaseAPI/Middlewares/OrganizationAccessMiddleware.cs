using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using DAL;
using DAL.RepositoryInterfaces;
using ShiftEaseAPI.Middlewares;

public class OrganizationAccessMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IServiceProvider _serviceProvider;

    public OrganizationAccessMiddleware(RequestDelegate next, IServiceProvider serviceProvider)
    {
        _next = next;
        _serviceProvider = serviceProvider;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var authorizationHeader = context.Request.Headers["Authorization"].FirstOrDefault();
        if (authorizationHeader == null || !authorizationHeader.StartsWith("Bearer "))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Authorization header is missing or invalid.");
            return;
        }

        var accessToken = authorizationHeader.Substring("Bearer ".Length).Trim();
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(accessToken);

        var userIdClaim = jwtToken?.Claims?.FirstOrDefault(c => c.Type == "UserId")?.Value;
        var roleClaim = jwtToken?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || string.IsNullOrEmpty(roleClaim))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Invalid token.");
            return;
        }

        var orgIdHeader = context.Request.Headers["X-Organization-Id"].FirstOrDefault();
        if (string.IsNullOrEmpty(orgIdHeader) || !int.TryParse(orgIdHeader, out var organizationId))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync("Invalid or missing Organization ID in headers.");
            return;
        }

        using var scope = _serviceProvider.CreateScope();
        var organizationRepository = scope.ServiceProvider.GetRequiredService<IOrganizationRepository>();
        var employerId = await organizationRepository.GetEmployerIdByOrganizationIdAsync(organizationId);

        if (roleClaim == "Employer")
        {
            // Employer must own the organization
            if (employerId != int.Parse(userIdClaim))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("You do not have access to this organization.");
                return;
            }
        }
        else if (roleClaim == "Employee")
        {
            var endpoint = context.GetEndpoint();
            var hasEmployeeAccess = endpoint?.Metadata.GetMetadata<AllowEmployeeAccessAttribute>() != null;
            
            if (!hasEmployeeAccess)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync("Employees are not allowed to perform this action.");
                return;
            }

            //TODO. Condition if employee belongs to organization
        }
        else
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsync("Unknown role.");
            return;
        }

        await _next(context);
    }
}

