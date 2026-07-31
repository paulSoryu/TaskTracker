using Mapster;
using WebApiTaskTracker.Business.Models.Tasks;
using WebApiTaskTracker.WebApi.DTOs.Tasks;

namespace WebApiTaskTracker.WebApi.DTOs.MappingConfigurations;

public class TaskMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // Configure mapping from TaskView to TaskResponse
        config.NewConfig<TaskView, TaskResponse>();

        // Configure mapping from TaskView to TaskSummaryResponse
        config.NewConfig<TaskView, TaskSummaryResponse>();

        // Configure mapping from from TaskCreateRequest to TaskSaveCommand
        config.NewConfig<TaskCreateRequest, TaskSaveCommand>()
            .Map(dest => dest.Id, src => (Guid?)null);

        // Configure mapping from TaskUpdateRequest to TaskSaveCommand
        config.NewConfig<TaskUpdateRequest, TaskSaveCommand>();
    }
}
