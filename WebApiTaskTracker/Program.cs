using FluentValidation;
using Mapster;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Text.Json.Serialization;
using WebApiTaskTracker.Business.Services.Auths;
using WebApiTaskTracker.Business.Services.Categories;
using WebApiTaskTracker.Business.Services.Emails;
using WebApiTaskTracker.Business.Services.Tasks;
using WebApiTaskTracker.DataAccess.Databases;
using WebApiTaskTracker.DataAccess.Entities;
using WebApiTaskTracker.Utilities;
using WebApiTaskTracker.WebApi.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container

// User context service to access the current user in the application
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserContext, HttpUserContext>();

// Identity and authentication
builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme).AddCookie(IdentityConstants.ApplicationScheme);
builder.Services.AddAuthorization();

builder.Services.AddIdentityCore<UserEntity>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedEmail = false;
})
    .AddEntityFrameworkStores<TaskTrackerDbContext>()
    .AddDefaultTokenProviders()
    .AddSignInManager<SignInManager<UserEntity>>();

// CORS configuration to allow requests from the frontend application running on a different origin
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Business services
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddSingleton<IEmailSender<UserEntity>, EmailSenderService>();

// Validators and exception handling
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Configure JSON options to use string representation for enums in the API responses
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// Database context and Mapster configuration
builder.Services.AddDbContext<TaskTrackerDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddMapster();

// Access by adding /swagger to the base URL of the API. For example, https://localhost:5001/swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddFluentValidationRulesToSwagger();

// Mapster check when mapping if the source member exists for the destination member. If not, it will throw an exception. This is useful to catch mapping issues early during development.
// TypeAdapterConfig.GlobalSettings.RequireDestinationMemberSource = true;
TypeAdapterConfig.GlobalSettings.Scan(Assembly.GetExecutingAssembly());
// TypeAdapterConfig.GlobalSettings.Compile();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<TaskTrackerDbContext>();
    context.Database.Migrate();
}

app.UseExceptionHandler();
// Custom middleware to handle 401 Unauthorized responses and return a JSON response instead of the default HTML response.
// It is needed because global exception handler middleware kicks in after the authentication middleware, so if a request is unauthorized, it will return a 401 response with an HTML page instead of a JSON response.
// This middleware will catch that and return a JSON response instead.
app.UseStatusCodePages(async context =>
{
    if (context.HttpContext.Response.StatusCode == StatusCodes.Status401Unauthorized)
    {
        context.HttpContext.Response.ContentType = "application/json";
        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            error = "Unauthorized",
            message = "Your session has expired or your token is invalid."
        });
    }
});

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

app.UseHttpsRedirection();
app.UseCors("FrontendPolicy");

app.UseAuthentication();
app.UseAuthorization();


app.MapAuthEndpoints();
app.MapTaskEndpoints();
app.MapCategoryEndpoints();

app.Run();