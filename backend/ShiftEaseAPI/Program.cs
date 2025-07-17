using System.Text;
using BLL.Interfaces;
using BLL.ServiceInterfaces;
using BLL.Services;
using DAL;
using DAL.Repositories;
using DAL.RepositoryInterfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();

//Allowing cors to address from front-end
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsAllowAll", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://shiftease-frontend")
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
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]))
        };
    });

// AppDbContext to DI
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
// Repositories to DI
builder.Services.AddScoped<IEmployerRepository, EmployerRepository>();
builder.Services.AddScoped<IOrganizationRepository, OrganizationRepository>();
builder.Services.AddScoped<IGroupRepository, GroupRepository>();
builder.Services.AddScoped<IShiftTypeRepository, ShiftTypeRepository>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IScheduleRepository, ScheduleRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IPasswordHasher<Employer>, PasswordHasher<Employer>>();
builder.Services.AddScoped<IEmployeeOptionsRepository, EmployeeOptionsRepository>();
builder.Services.AddScoped<ISupportRepository, SupportRepository>();
// Services to DI
builder.Services.AddScoped<IIdentityService, IdentityService>();
builder.Services.AddScoped<IOrganizationService, OrganizationService>();
builder.Services.AddScoped<IGroupService, GroupService>();
builder.Services.AddScoped<IShiftTypeService, ShiftTypeService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IScheduleService, ScheduleService>();
builder.Services.AddScoped<IEmployeeOptionsService, EmployeeOptionsService>();
builder.Services.AddScoped<ISupportRepository, SupportRepository>();


// For Swagger and api versioning
builder.Services.AddControllersWithViews();
builder.Services.AddApiVersioning();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
});



// Building App
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.UseRouting();

// Use CORS, Authentication, and Authorization
app.UseCors("CorsAllowAll");
app.UseAuthentication();
app.UseAuthorization();

// Register OrganizationAccessMiddleware selectively
app.UseWhen(context =>
        context.Request.Path.StartsWithSegments("/api") &&
        !context.Request.Path.StartsWithSegments("/api/identity") &&
        !context.Request.Path.StartsWithSegments("/api/organization"),
    appBuilder =>
    {
        appBuilder.UseMiddleware<OrganizationAccessMiddleware>();
    });

//Use swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
    c.RoutePrefix = string.Empty; //Allowing swagger to be in root directory
});

// Map controllers and API endpoints
app.MapControllers();

// Settings for Development (OpenAPI)
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.Run();
