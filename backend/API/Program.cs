using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.RateLimiting;
using BLL.Contracts;
using BLL.Exceptions;
using BLL.Services;
using DAL;
using DAL.Contracts;
using DAL.Repositories;
using DAL.Repositories.Employee.Preferences;
using DAL.Repositories.PersonalDays;
using DAL.Repositories.SickLeave;
using DTOs.EmployeeDtos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();

// Rate limiting — protects auth endpoints from brute-force
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("auth-login", o =>
    {
        o.PermitLimit = 15;
        o.Window = TimeSpan.FromMinutes(1);
        o.QueueLimit = 0;
    });
    options.AddFixedWindowLimiter("auth-register", o =>
    {
        o.PermitLimit = 5;
        o.Window = TimeSpan.FromMinutes(1);
        o.QueueLimit = 0;
    });
    options.AddFixedWindowLimiter("auth-forgot-password", o =>
    {
        o.PermitLimit = 15;
        o.Window = TimeSpan.FromMinutes(5);
        o.QueueLimit = 0;
    });
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// CORS origins are configured via CORS__Origins (semicolon-separated) env var
// e.g. CORS__Origins=http://localhost:3000;http://localhost:3006
var corsOrigins = builder.Configuration["Cors:Origins"]
    ?.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    ?? ["http://localhost:3000", "http://localhost:3006"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsAllowAll", policy =>
    {
        policy.WithOrigins(corsOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

//Configuring Jwt token
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                ctx.Token = ctx.Request.Cookies["accessToken"];
                return Task.CompletedTask;
            }
        };
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"])),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// Data Protection: keys are persisted to the PostgreSQL database (DataProtectionKeys table).
// This means the database must be running and the connection string must be set BEFORE
// the app starts — keys are written on first boot and must survive container restarts.
// Never rotate or delete these keys; doing so will invalidate all encrypted PII data.
builder.Services.AddDataProtection()
    .PersistKeysToDbContext<AppDbContext>()
    .SetApplicationName("ShiftEase");

// AppDbContext to DI
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"), npgsql =>
    {
        npgsql.MigrationsAssembly("DAL");
    });
});

NpgsqlConnection.GlobalTypeMapper.MapEnum<DayOfWeek>();

// Repositories to DI
builder.Services.AddScoped<IEmployerRepository, EmployerRepository>();
builder.Services.AddScoped<IOrganizationRepository, OrganizationRepository>();
builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddScoped<IShiftTemplateRepository, ShiftTemplateRepository>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IScheduleRepository, ScheduleRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IPasswordResetRepository, PasswordResetRepository>();
builder.Services.AddScoped<IPasswordHasher<Employer>, PasswordHasher<Employer>>();
builder.Services.AddScoped<IVacationRepository, VacationRepository>();
builder.Services.AddScoped<IVacationRequestRepository, VacationRequestRepository>();
builder.Services.AddScoped<ISickLeaveRepository, SickLeaveRepository>();
builder.Services.AddScoped<ISickLeaveRequestRepository, SickLeaveRequestRepository>();
builder.Services.AddScoped<IPersonalDayRepository, PersonalDayRepository>();
builder.Services.AddScoped<IPersonalDayRequestRepository, PersonalDayRequestRepository>();
builder.Services.AddScoped<IWeekDayPreferenceRepository, WeekDayPreferenceRepository>();
builder.Services.AddScoped<IShiftTypePreferenceRepository, ShiftTypePreferenceRepository>();
// Services to DI
builder.Services.AddScoped<OrganizationOwnerFilter>();
builder.Services.AddScoped<IIdentityService, IdentityService>();
builder.Services.AddScoped<IOrganizationService, OrganizationService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IShiftTemplateService, ShiftTemplateService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IScheduleService, ScheduleService>();
builder.Services.AddScoped<IVacationService, VacationService>();
builder.Services.AddScoped<ISickLeaveService, SickLeaveService>();
builder.Services.AddScoped<IPersonalDayService, PersonalDayService>();
builder.Services.AddScoped<IPreferenceService, PreferenceService>();
builder.Services.AddScoped<ISupportService, SupportService>();
builder.Services.AddScoped<IFeedbackService, FeedbackService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IScheduleGeneratorService, GreedyScheduleGeneratorService>();
builder.Services.AddScoped<IAcoScheduleGeneratorService, AcoScheduleGeneratorService>();
builder.Services.AddScoped<IGaScheduleGeneratorService, GaScheduleGeneratorService>();
builder.Services.AddScoped<IAcoGaScheduleGeneratorService, AcoGaScheduleGeneratorService>();
builder.Services.AddScoped<IScheduleExportService, ScheduleExportService>();
builder.Services.AddScoped<IMailService, MailService>();
builder.Services.AddHostedService<RefreshTokenCleanupService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddSingleton<ILaborRulesProvider, LaborRulesProvider>();

// For Swagger and api versioning
builder.Services.AddControllersWithViews();
builder.Services.AddApiVersioning();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Shift Ease API", Version = "v1" });
});

// Building App
var app = builder.Build();

//For automat migrations
await app.ApplyMigrationsAsync();

app.UseRouting();
app.UseRateLimiter();

// Use CORS, Authentication, and Authorization
app.UseCors("CorsAllowAll");

app.UseAuthentication();
app.UseAuthorization();


//Use swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
    c.RoutePrefix = string.Empty; //Allowing swagger to be in root directory
});

// Map controllers and API endpoints
app.MapControllers();

//Use Custom Exceptions
app.UseExceptionHandler(appBuilder => appBuilder.Run(async context =>
{
    var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;

    var (status, errorCode, message, errors) = exception switch
    {
        ValidationException e  => (400, e.ErrorCode, e.Message, (object?)e.Errors),
        UnauthorizedException e => (401, e.ErrorCode, e.Message, (object?)null),
        ForbiddenException e   => (403, e.ErrorCode, e.Message, (object?)null),
        NotFoundException e    => (404, e.ErrorCode, e.Message, (object?)null),
        _                      => (500, "INTERNAL_ERROR", "Internal server error", (object?)null)
    };

    context.Response.StatusCode = status;
    context.Response.ContentType = "application/json";
    await context.Response.WriteAsJsonAsync(new { errorCode, message, errors });
}));

// Settings for Development (OpenAPI)
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.Run();

// Expose Program to the test project (WebApplicationFactory<Program>)
public partial class Program { }
