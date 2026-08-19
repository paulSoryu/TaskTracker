using FluentResults;
using Mapster;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;
using TaskTracker.Api.DTOs;
using TaskTracker.Api.DTOs.Tasks;
using TaskTracker.Api.Extensions;
using TaskTracker.Business.Models;
using TaskTracker.Business.Models.Auths;
using TaskTracker.Business.Models.Tasks;
using TaskTracker.Business.Services.Tasks;
using TaskTracker.Business.Services.Users;

namespace TaskTracker.Api.Endpoints;

public static class AdminEndpoints
{
    public static void MapAdminEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/admin")
            .RequireAuthorization(policy => policy.RequireRole("Admin")) // Доступ только для роли Admin
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapGet("/users", GetAllUsers)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapPost("/assign-role", AssignRole)
            .WithValidation<AssignRoleRequest>() // Если используете FluentValidation / кастомную валидацию
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/toggle-block/{id}", ToggleBlockUser)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/delete-user/{id}", DeleteUserByAdmin)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<Ok<PagedResult<UserInfoView>>> GetAllUsers(IUserService userService, [AsParameters] GetUsersRequest request)
    {
        var filterQuery = request.Adapt<FilterUsersQuery>();
        var sortQuery = request.Adapt<SortUsersQuery>();
        var paginationQuery = request.Adapt<PaginateUsersQuery>();
        PagedResult<UserInfoView> pagedUsers = await userService.GetAllAsync(filterQuery, sortQuery, paginationQuery);

        var response = new PagedResult<UserListResponse>(
            pagedTasks.Items.Adapt<IReadOnlyCollection<UserListResponse>>(),
            pagedTasks.TotalCount);

        return TypedResults.Ok(response);
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> AssignRole(AssignRoleRequest request, IUserService userService)
    {
        Result result = await userService.AssignRoleToUserAsync(request.UserId, request.RoleName);
        return result.ToTypedHttpResult();
    }

    private static async Task<Results<Ok<UserBlockStatusView>, ProblemHttpResult>> ToggleBlockUser(string id, IUserService userService)
    {
        Result<UserBlockStatusView> result = await userService.ToggleUserBlockStatusAsync(id);
        return result.ToTypedHttpResult();
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> SendDeletionWarning(string id, string reason, IUserCoordinator userCoordinator)
    {
        Result result = await userCoordinator.SendDeletionWarningLetterAsync(id, reason);
        return result.ToTypedHttpResult();
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> DeleteUserByAdmin(string id, string reason, IUserCoordinator userCoordinator)
    {


        Result result = await userCoordinator.DeleteUserAndDataByAdminAsync(id, reason);
        return result.ToTypedHttpResult();
    }
}
