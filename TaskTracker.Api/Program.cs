using FluentValidation;
using Mapster;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Text.Json.Serialization;
using TaskTracker.Api.Endpoints;
using TaskTracker.Api.Infrastructure;
using TaskTracker.Api.Middleware;
using TaskTracker.Api.Utilities;
using TaskTracker.Business.Services.Auths;
using TaskTracker.Business.Services.Categories;
using TaskTracker.Business.Services.Emails;
using TaskTracker.Business.Services.Identity;
using TaskTracker.Business.Services.Reordering.Factories;
using TaskTracker.Business.Services.Reordering.Strategies;
using TaskTracker.Business.Services.Tasks;
using TaskTracker.Business.Services.Users;
using TaskTracker.DataAccess.Databases;
using TaskTracker.DataAccess.Entities;
using TaskTracker.Shared.Utilities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container

// User context service to access the current user in the application
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserContext, HttpUserContext>();

// Identity and authentication
builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme).AddCookie(IdentityConstants.ApplicationScheme);
builder.Services.AddAuthorization();

builder.Services.AddMemoryCache();

builder.Services.AddIdentityCore<UserEntity>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedEmail = false;
    options.Lockout.AllowedForNewUsers = true;
})
    .AddRoles<IdentityRole<Guid>>()
    .AddEntityFrameworkStores<TaskTrackerDbContext>()
    .AddDefaultTokenProviders()
    .AddRoleManager<RoleManager<IdentityRole<Guid>>>()
    .AddSignInManager<SignInManager<UserEntity>>();

// This is needed to logout admins as soon as they are no longer admins
builder.Services.AddScoped<ISecurityStampValidator, SecurityStampValidator<UserEntity>>();

// Check Security Stamps every minute, this is important for removing admin roles. By default it's 30 minutes which is too long
builder.Services.Configure<SecurityStampValidatorOptions>(options =>
{
    options.ValidationInterval = TimeSpan.Zero;
});

// CORS configuration to allow requests from the frontend application running on a different origin
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://127.0.0.1:3000", "https://192.168.0.102:3000")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});

// Business services
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserCoordinator, UserCoordinator>();
builder.Services.AddSingleton<IEmailSenderService<UserEntity>, EmailSenderService>();

// Adapt custom interface to Identity interface, as IEmailSender can't work directly in a class library like our Business layer
builder.Services.AddSingleton<IEmailSender<UserEntity>>(sp =>
{
    var customSender = sp.GetRequiredService<IEmailSenderService<UserEntity>>();
    return new IdentityEmailSenderProxy(customSender);
});

// Reordering strategies
builder.Services.AddScoped(typeof(CustomOrderReorderingStrategy<>));
builder.Services.AddScoped(typeof(SortedListReorderingStrategy<>));
builder.Services.AddScoped(typeof(IReorderingStrategyFactory<>), typeof(ReorderingStrategyFactory<>));

// Dependency inversion to get UserEntity for SignInManager in API layer
builder.Services.AddScoped<IIdentitySessionManager, IdentitySessionManager>();

// Validators and exception handling
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Configure JSON options to use string representation for enums in the API responses
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNameCaseInsensitive = true;
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// Database context and Mapster configuration
builder.Services.AddDbContext<TaskTrackerDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"),
    b => b.MigrationsAssembly("TaskTracker.DataAccess")));
builder.Services.AddMapster();

// Access by adding /swagger to the base URL of the API. For example, https://localhost:5001/swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddFluentValidationRulesToSwagger();

TypeAdapterConfig.GlobalSettings.Scan(Assembly.GetExecutingAssembly());

var app = builder.Build();

// Automatically apply migrations
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<TaskTrackerDbContext>();
    context.Database.Migrate();
}

// Ensure at least one admin account exists
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await IdentitySeedData.EnsureAtLeastOneAdminAsync(services);
}

app.UseExceptionHandler();
app.UseStatusCodePages();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{

    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        // Explicitly forbid saving authorization in the browser's localStorage
        options.ConfigObject.AdditionalItems["persistAuthorization"] = false;
    });
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseCors("FrontendPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<UserActivityMiddleware>();

app.MapAuthEndpoints();
app.MapTaskEndpoints();
app.MapCategoryEndpoints();
app.MapAdminEndpoints();

app.Run();