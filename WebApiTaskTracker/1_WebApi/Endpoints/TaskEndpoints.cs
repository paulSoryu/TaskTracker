using Mapster;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;
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
        TaskBusinessModel? businessTask = await taskService.GetByIdAsync(id);

        if (businessTask is null)
            return TypedResults.NotFound($"Task with ID {id} not found.");

        var response = businessTask.Adapt<TaskResponse>();
        return TypedResults.Ok(response);
    }

    private static async Task<Ok<IReadOnlyCollection<TaskSummaryResponse>>> GetAllTasks(ITaskService taskService, [AsParameters] GetTasksRequest request)
    {
        var query = request.Adapt<GetTasksQuery>();
        IReadOnlyCollection<TaskBusinessModel> businessTasks = await taskService.GetAllAsync(query);

        var response = businessTasks.Adapt<IReadOnlyCollection<TaskSummaryResponse>>();
        return TypedResults.Ok(response);
    }

    private static async Task<CreatedAtRoute<TaskResponse>> CreateTask(
        TaskCreateRequest taskRequest,
        ITaskService taskService,
        ClaimsPrincipal user)
    {
        var command = taskRequest.Adapt<TaskSaveCommand>();
        Guid userId = user.GetUserId();

        TaskBusinessModel createdBusinessTask = await taskService.CreateAsync(command, userId);
        var response = createdBusinessTask.Adapt<TaskResponse>();

        return TypedResults.CreatedAtRoute(response, "GetTaskById", new { id = response.Id });
    }

    private static async Task<NoContent> UpdateTask(
        Guid id,
        TaskUpdateRequest taskRequest,
        ITaskService taskService)
    {
        var command = taskRequest.Adapt<TaskSaveCommand>() with { Id = id };

        await taskService.UpdateAsync(command);
        return TypedResults.NoContent();
    }

    private static async Task<NoContent> DeleteTask(Guid id, ITaskService taskService)
    {
        await taskService.DeleteAsync(id);
        return TypedResults.NoContent();
    }
}
