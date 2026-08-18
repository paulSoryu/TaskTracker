using Mapster;
using TaskTracker.Api.DTOs.Tasks;
using TaskTracker.Business.Models.Tasks;

namespace TaskTracker.Api.DTOs.MappingConfigurations;

public class TaskMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // Configure mapping from TaskView to TaskResponse
        config.NewConfig<TaskView, TaskResponse>();

        // Configure mapping from TaskView to TaskListResponse
        config.NewConfig<TaskView, TaskListResponse>();

        // Configure mapping from from CreateTaskRequest to SaveTaskCommand
        config.NewConfig<CreateTaskRequest, SaveTaskCommand>()
            .Map(dest => dest.Id, src => (Guid?)null);

        // Configure mapping from UpdateTaskRequest to SaveTaskCommand
        config.NewConfig<UpdateTaskRequest, SaveTaskCommand>();

        // Configure mapping from MoveTaskRequest to MoveTaskCommand and SortTasksQuery
        config.NewConfig<MoveTaskRequest, MoveTaskCommand>();
        config.NewConfig<MoveTaskRequest, SortTasksQuery>();

        // Break down GetTasksRequest into its components for filtering, sorting, and pagination
        config.NewConfig<GetTasksRequest, FilterTasksQuery>();
        config.NewConfig<GetTasksRequest, SortTasksQuery>();
        config.NewConfig<GetTasksRequest, PaginateTasksQuery>();
    }
}
