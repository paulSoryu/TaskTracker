using FluentResults;
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
        var routeGroup = endpoints.MapGroup("/api/tasks")
            .RequireAuthorization()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        routeGroup.MapGet("/", GetAllTasks);

        routeGroup.MapGet("/{id:Guid}", GetTaskById)
            .WithName("GetTaskById")
            .ProducesProblem(StatusCodes.Status404NotFound);

        routeGroup.MapPost("/", CreateTask)
            .WithValidation<CreateTaskRequest>()
            .ProducesProblem(StatusCodes.Status400BadRequest);

        routeGroup.MapPut("/{id:Guid}", UpdateTask)
            .WithValidation<UpdateTaskRequest>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        routeGroup.MapDelete("/{id:Guid}", DeleteTask)
            .ProducesProblem(StatusCodes.Status404NotFound);

        routeGroup.MapPatch("/reorder", ReorderTask)
            .WithValidation<MoveTaskRequest>()
            .ProducesProblem(StatusCodes.Status400BadRequest);
    }

    private static async Task<Ok<IReadOnlyCollection<TaskListResponse>>> GetAllTasks(ITaskService taskService, [AsParameters] GetTasksRequest request)
    {
        var query = request.Adapt<GetTasksQuery>();
        IReadOnlyCollection<TaskView> businessTasks = await taskService.GetAllAsync(query);

        var response = businessTasks.Adapt<IReadOnlyCollection<TaskListResponse>>();
        return TypedResults.Ok(response);
    }

    private static async Task<Results<Ok<TaskResponse>, ProblemHttpResult>> GetTaskById(Guid id, ITaskService taskService)
    {
        Result<TaskView> result = await taskService.GetByIdAsync(id);

        Result<TaskResponse> responseResult = result.Map(task => task.Adapt<TaskResponse>());

        return responseResult.ToTypedHttpResult();
    }

    private static async Task<Results<CreatedAtRoute<TaskResponse>, ProblemHttpResult>> CreateTask(CreateTaskRequest taskRequest, ITaskService taskService, ClaimsPrincipal user)
    {
        var command = taskRequest.Adapt<SaveTaskCommand>();
        Guid userId = user.GetUserId();

        Result<TaskView> result = await taskService.CreateAsync(command, userId);

        Result<TaskResponse> responseResult = result.Map(task => task.Adapt<TaskResponse>());

        return responseResult.ToCreatedAtRouteResult(
            routeName: "GetTaskById",
            routeValues: new { id = responseResult.ValueOrDefault?.Id });
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> UpdateTask(Guid id, UpdateTaskRequest taskRequest, ITaskService taskService)
    {
        var command = taskRequest.Adapt<SaveTaskCommand>() with { Id = id };

        Result result = await taskService.UpdateAsync(command);

        return result.ToTypedHttpResult();
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> DeleteTask(Guid id, ITaskService taskService)
    {
        Result result = await taskService.DeleteAsync(id);

        return result.ToTypedHttpResult();
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> ReorderTask(MoveTaskRequest moveRequest, ITaskService taskService)
    {
        var command = moveRequest.Adapt<MoveTaskCommand>();
        Result result = await taskService.ReorderTaskAsync(command);
        return result.ToTypedHttpResult();
    }
}
