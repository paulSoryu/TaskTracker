using FluentResults;
using Mapster;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskTracker.Api.DTOs.Admin;
using TaskTracker.Api.DTOs.Users;
using TaskTracker.Api.Extensions;
using TaskTracker.Business.Models;
using TaskTracker.Business.Models.Users;
using TaskTracker.Business.Services.Seeding;
using TaskTracker.Business.Services.Users;
using TaskTracker.DataAccess.Entities;

namespace TaskTracker.Api.Endpoints;

public static class AdminEndpoints
{
    public static void MapAdminEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/admin")
            .RequireAuthorization(policy => policy.RequireRole("Admin"))
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapGet("/users", GetAllUsers)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapPost("/users/{id}/assign-admin", AssignAdmin)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/users/{id}/remove-admin", RemoveAdmin)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/users/{id}/block", BlockUser)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/users/{id}/unblock", UnblockUser)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/users/{id}/send-deletion-warning", SendDeletionWarning)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/users/{id}/delete", DeleteUserByAdmin)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/seed-demo-data", SeedDemoData)
            .ProducesProblem(StatusCodes.Status400BadRequest);
    }

    private static async Task<Ok<PagedResult<UserListResponse>>> GetAllUsers(IUserService userService, [AsParameters] GetUsersRequest request)
    {
        var filterQuery = request.Adapt<FilterUsersQuery>();
        var sortQuery = request.Adapt<SortUsersQuery>();
        var paginateQuery = request.Adapt<PaginateUsersQuery>();
        PagedResult<UserView> pagedUsers = await userService.GetAllAsync(filterQuery, sortQuery, paginateQuery);

        var response = new PagedResult<UserListResponse>(
            pagedUsers.Items.Adapt<IReadOnlyCollection<UserListResponse>>(),
            pagedUsers.TotalCount);

        return TypedResults.Ok(response);
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> AssignAdmin(Guid id, IUserService userService)
    {
        var result = await userService.AssignAdminRoleAsync(id);
        return result.ToTypedHttpResult();
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> RemoveAdmin(Guid id, IUserService userService)
    {
        var result = await userService.RemoveAdminRoleAsync(id);
        return result.ToTypedHttpResult();
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> BlockUser(Guid id, BlockUserRequest request, IUserService userService)
    {
        Result result = await userService.BlockUserAsync(id, request.Until);
        return result.ToTypedHttpResult();
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> UnblockUser(Guid id, IUserService userService)
    {
        Result result = await userService.UnblockUserAsync(id);
        return result.ToTypedHttpResult();
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> SendDeletionWarning(Guid id, UserDeletionRequest request, IUserCoordinator userCoordinator)
    {
        Result result = await userCoordinator.SendDeletionWarningLetterAsync(id, request.Reason);
        return result.ToTypedHttpResult();
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> DeleteUserByAdmin(Guid id, [FromBody] UserDeletionRequest request, IUserCoordinator userCoordinator, UserManager<UserEntity> userManager, ClaimsPrincipal currentUser)
    {
        var currentUserId = userManager.GetUserId(currentUser)!;

        Result result = await userCoordinator.DeleteUserAndDataByAdminAsync(id, request.Reason, currentUserId);
        return result.ToTypedHttpResult();
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> SeedDemoData(ClaimsPrincipal user, SeedDemoDataRequest request, IDataSeederService seeder, UserManager<UserEntity> userManager)
    {
        var userId = userManager.GetUserId(user)!;

        Result result = await seeder.GenerateDefaultDataAsync(Guid.Parse(userId), request.TaskAddAmount, request.CategoryAddAmount);

        return result.ToTypedHttpResult();
    }
}