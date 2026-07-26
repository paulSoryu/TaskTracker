using System.Security.Claims;
using WebApiTaskTracker.Business.Services.Tasks;
using WebApiTaskTracker.Utilities;
using WebApiTaskTracker.WebApi.DTOs;
using WebApiTaskTracker.WebApi.DTOs.Tasks;

namespace WebApiTaskTracker.WebApi.Endpoints;

// By passing DTOs directly to the service layer, we are breaking the single responsibility principle, as the service layer is now responsible for both business logic and data transfer.
// However, this is a common practice in simple applications to reduce boilerplate code and improve maintainability.
// In a more complex application, it would be better to use separate DTOs for the service layer and the API layer, and map between them using a mapping library like Mapster.

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
