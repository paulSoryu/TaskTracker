using FluentResults;
using Mapster;
using Microsoft.AspNetCore.Http.HttpResults;
using TaskTracker.Api.DTOs.Users;
using TaskTracker.Business.Models;
using TaskTracker.Business.Models.Users;
using TaskTracker.Business.Services.Users;

namespace TaskTracker.Api.Endpoints;

public static class AdminEndpoints
{
    public static void MapAdminEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/admin")
            //.RequireAuthorization(policy => policy.RequireRole("Admin"))
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapGet("/users", GetAllUsers)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        //group.MapPost("/assign-role", AssignRole)
        //    .WithValidation<AssignRoleRequest>()
        //    .ProducesProblem(StatusCodes.Status400BadRequest)
        //    .ProducesProblem(StatusCodes.Status404NotFound);

        //group.MapPost("/toggle-block/{id}", ToggleBlockUser)
        //    .ProducesProblem(StatusCodes.Status400BadRequest)
        //    .ProducesProblem(StatusCodes.Status404NotFound);

        //group.MapDelete("/delete-user/{id}", DeleteUserByAdmin)
        //    .ProducesProblem(StatusCodes.Status400BadRequest)
        //    .ProducesProblem(StatusCodes.Status404NotFound);
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

    //private static async Task<Results<NoContent, ProblemHttpResult>> AssignRole(AssignRoleRequest request, IUserService userService)
    //{
    //    Result result = await userService.AssignRoleToUserAsync(request.UserId, request.RoleName);
    //    return result.ToTypedHttpResult();
    //}

    //private static async Task<Results<Ok<UserBlockStatusView>, ProblemHttpResult>> ToggleBlockUser(string id, IUserService userService)
    //{
    //    Result<UserBlockStatusView> result = await userService.ToggleUserBlockStatusAsync(id);
    //    return result.ToTypedHttpResult();
    //}

    //private static async Task<Results<NoContent, ProblemHttpResult>> SendDeletionWarning(string id, string reason, IUserCoordinator userCoordinator)
    //{
    //    Result result = await userCoordinator.SendDeletionWarningLetterAsync(id, reason);
    //    return result.ToTypedHttpResult();
    //}

    //private static async Task<Results<NoContent, ProblemHttpResult>> DeleteUserByAdmin(string id, string reason, IUserCoordinator userCoordinator)
    //{
    //    Result result = await userCoordinator.DeleteUserAndDataByAdminAsync(id, reason);
    //    return result.ToTypedHttpResult();
    //}
}
