using Mapster;
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

    private static async Task<IResult> GetTaskById(Guid id, ITaskService taskService)
    {
        TaskBusinessModel? businessTask = await taskService.GetByIdAsync(id);

        if (businessTask is null)
            return Results.NotFound(new { Message = $"Задача с ID {id} не найдена." });

        var response = businessTask.Adapt<TaskResponse>();
        return Results.Ok(response);
    }

    private static async Task<IResult> GetAllTasks(ITaskService taskService)
    {
        IReadOnlyCollection<TaskBusinessModel> businessTasks = await taskService.GetAllAsync();

        var response = businessTasks.Adapt<IReadOnlyCollection<TaskSummaryResponse>>();
        return Results.Ok(response);
    }

    private static async Task<IResult> CreateTask(
        TaskCreateRequest taskRequest,
        ITaskService taskService,
        ClaimsPrincipal user)
    {
        var command = taskRequest.Adapt<TaskSaveCommand>();
        Guid userId = user.GetUserId();

        TaskBusinessModel createdBusinessTask = await taskService.CreateAsync(command, userId);
        var response = createdBusinessTask.Adapt<TaskResponse>();

        return Results.CreatedAtRoute("GetTaskById", new { id = response.Id }, response);
    }

    private static async Task<IResult> UpdateTask(
        Guid id,
        TaskUpdateRequest taskRequest,
        ITaskService taskService)
    {
        var command = taskRequest.Adapt<TaskSaveCommand>() with { Id = id };

        await taskService.UpdateAsync(command);
        return Results.NoContent();
    }

    private static async Task<IResult> DeleteTask(Guid id, ITaskService taskService)
    {
        await taskService.DeleteAsync(id);
        return Results.NoContent();
    }
}
