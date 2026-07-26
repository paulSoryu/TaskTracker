using FluentValidation;
using Mapster;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;
using WebApiTaskTracker.Data.Databases;
using WebApiTaskTracker.Data.Entities;
using WebApiTaskTracker.DTOs.MappingConfigurations;
using WebApiTaskTracker.Endpoints;
using WebApiTaskTracker.Services.Categories;
using WebApiTaskTracker.Services.Emails;
using WebApiTaskTracker.Services.Tasks;
using WebApiTaskTracker.Services.Users;
using WebApiTaskTracker.Utilities;

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserContext, HttpUserContext>();

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();
builder.Services.AddIdentityApiEndpoints<UserEntity>(options => {
    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedEmail = false;
}).AddEntityFrameworkStores<TaskTrackerDbContext>();

builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddSingleton<IEmailSender<UserEntity>, EmailSenderService>();

builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddDbContext<TaskTrackerDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddMapster();

// Access by adding /swagger to the base URL of the API. For example, https://localhost:5001/swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddFluentValidationRulesToSwagger();

// Mapster check when mapping if the source member exists for the destination member. If not, it will throw an exception. This is useful to catch mapping issues early during development.
//TypeAdapterConfig.GlobalSettings.RequireDestinationMemberSource = true;
TypeAdapterConfig.GlobalSettings.Scan(Assembly.GetExecutingAssembly());
//TypeAdapterConfig.GlobalSettings.Compile();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<TaskTrackerDbContext>();
    context.Database.Migrate();
}

app.UseExceptionHandler();
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

app.UseAuthentication();
app.UseAuthorization();

app.MapIdentityApi<UserEntity>();

app.MapTaskEndpoints();
app.MapCategoryEndpoints();

app.MapPost("/logout", async (
    ClaimsPrincipal userPrincipal,
    UserManager<UserEntity> userManager) =>
{
    var user = await userManager.GetUserAsync(userPrincipal);
    if (user == null) return Results.Unauthorized();

    await userManager.RemoveAuthenticationTokenAsync(
        user,
        "[AspNetCoreIdentityBearerToken]",
        "refresh_token");

    return Results.Ok(new { message = "Logout succesful. Refresh token is deleted." });
})
.RequireAuthorization();

app.Run();