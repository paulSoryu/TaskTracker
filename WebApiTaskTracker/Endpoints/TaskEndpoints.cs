using System.Security.Claims;
using WebApiTaskTracker.DTOs;
using WebApiTaskTracker.DTOs.Tasks;
using WebApiTaskTracker.Services.Tasks;
using WebApiTaskTracker.Utilities;

namespace WebApiTaskTracker.Endpoints;

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

    private static async Task<IResult> GetAllTasks(ITaskService taskService)
    {
        var tasks = await taskService.GetAllAsync();
        return Results.Ok(tasks);
    }

    private static async Task<IResult> GetTaskById(Guid id, ITaskService taskService)
    {
        var task = await taskService.GetByIdAsync(id);
        return Results.Ok(task);
    }

    private static async Task<IResult> CreateTask(TaskCreateRequest taskRequest, ITaskService taskService, ClaimsPrincipal user)
    {
        TaskResponse createdTask = await taskService.CreateAsync(taskRequest, user.GetUserId());
        return Results.CreatedAtRoute("GetTaskById", new { id = createdTask.Id }, createdTask);
    }

    private static async Task<IResult> UpdateTask(Guid id, TaskUpdateRequest taskRequest, ITaskService taskService)
    {
        await taskService.UpdateAsync(id, taskRequest);
        return Results.NoContent();
    }

    private static async Task<IResult> DeleteTask(Guid id, ITaskService taskService)
    {
        await taskService.DeleteAsync(id);
        return Results.NoContent();
    }
}
