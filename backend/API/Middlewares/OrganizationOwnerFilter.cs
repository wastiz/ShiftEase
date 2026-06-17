using BLL.Contracts;
using BLL.Helpers;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class OrganizationOwnerFilter : IAsyncActionFilter
{
    private readonly IOrganizationService _organizationService;

    public OrganizationOwnerFilter(IOrganizationService organizationService)
    {
        _organizationService = organizationService;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext ctx, ActionExecutionDelegate next)
    {
        var role = ctx.HttpContext.User.GetRole();

        if (role == UserRole.Employer)
        {
            if (ctx.ActionArguments.TryGetValue("orgId", out var orgIdObj)
                && int.TryParse(orgIdObj?.ToString(), out var orgId))
            {
                var userId = ctx.HttpContext.User.GetUserId();
                var belongs = await _organizationService.IsOrganizationBelongsToEmployerAsync(orgId, userId);

                if (!belongs)
                {
                    ctx.Result = new ObjectResult(new ErrorResponse("FORBIDDEN", "You do not have access to this organization."))
                    {
                        StatusCode = StatusCodes.Status403Forbidden
                    };
                    return;
                }
            }
        }

        await next();
    }
}