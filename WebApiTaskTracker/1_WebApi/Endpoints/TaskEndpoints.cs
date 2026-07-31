using FluentResults;
using Mapster;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;
using WebApiTaskTracker.Business.FluentErrors;
using WebApiTaskTracker.Business.Models.Tasks;
using WebApiTaskTracker.Business.Services.Tasks;
using WebApiTaskTracker.Utilities;
using WebApiTaskTracker.WebApi.DTOs;
using WebApiTaskTracker.WebApi.DTOs.Tasks;

namespace WebApiTaskTracker.WebApi.Endpoints;

public static class TaskEndpoints
{
    public static void MapTaskEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var routeGroup = endpoints.MapGroup("/api/tasks").RequireAuthorization();

        routeGroup.MapGet("/", GetAllTasks);

        routeGroup.MapGet("/{id:Guid}", GetTaskById)
            .WithName("GetTaskById");

        routeGroup.MapPost("/", CreateTask)
            .AddEndpointFilter<ValidationFilter<TaskCreateRequest>>();

        routeGroup.MapPut("/{id:Guid}", UpdateTask)
            .AddEndpointFilter<ValidationFilter<TaskUpdateRequest>>();

        routeGroup.MapDelete("/{id:Guid}", DeleteTask);
    }

    private static async Task<Results<Ok<TaskResponse>, NotFound<string>>> GetTaskById(Guid id, ITaskService taskService)
    {
        Result<TaskBusinessModel> result = await taskService.GetByIdAsync(id);

        if (result.IsFailed)
        {
            string errorMessage = result.Errors.First().Message;
            return TypedResults.NotFound(errorMessage);
        }

        var response = result.Value.Adapt<TaskResponse>();
        return TypedResults.Ok(response);
    }

    private static async Task<Ok<IReadOnlyCollection<TaskSummaryResponse>>> GetAllTasks(ITaskService taskService, [AsParameters] GetTasksRequest request)
    {
        var query = request.Adapt<GetTasksQuery>();
        IReadOnlyCollection<TaskBusinessModel> businessTasks = await taskService.GetAllAsync(query);

        var response = businessTasks.Adapt<IReadOnlyCollection<TaskSummaryResponse>>();
        return TypedResults.Ok(response);
    }

    private static async Task<Results<CreatedAtRoute<TaskResponse>, BadRequest<string>>> CreateTask(TaskCreateRequest taskRequest, ITaskService taskService, ClaimsPrincipal user)
    {
        var command = taskRequest.Adapt<TaskSaveCommand>();
        Guid userId = user.GetUserId();

        Result<TaskBusinessModel> result = await taskService.CreateAsync(command, userId);

        if (result.IsFailed)
            return TypedResults.BadRequest(result.Errors.First().Message);

        var response = result.Value.Adapt<TaskResponse>();
        return TypedResults.CreatedAtRoute(response, "GetTaskById", new { id = response.Id });
    }

    private static async Task<Results<NoContent, NotFound<string>, BadRequest<string>>> UpdateTask(Guid id, TaskUpdateRequest taskRequest, ITaskService taskService)
    {
        var command = taskRequest.Adapt<TaskSaveCommand>() with { Id = id };

        Result result = await taskService.UpdateAsync(command);

        if (result.IsFailed)
        {
            if (result.HasError<NotFoundError>())
                return TypedResults.NotFound(result.Errors.First().Message);

            if (result.HasError<ValidationError>())
                return TypedResults.BadRequest(result.Errors.First().Message);

            return TypedResults.BadRequest("An unexpected error occurred.");
        }

        return TypedResults.NoContent();
    }

    private static async Task<Results<NoContent, NotFound<string>>> DeleteTask(Guid id, ITaskService taskService)
    {
        Result result = await taskService.DeleteAsync(id);

        if (result.IsFailed)
            return TypedResults.NotFound(result.Errors.First().Message);

        return TypedResults.NoContent();
    }
}
