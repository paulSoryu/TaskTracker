using Mapster;
using WebApiTaskTracker.Business.Models.Tasks;
using WebApiTaskTracker.WebApi.DTOs.Tasks;

namespace WebApiTaskTracker.WebApi.DTOs.MappingConfigurations;

public class TaskMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // Configure mapping from TaskBusinessModel to TaskResponse
        config.NewConfig<TaskBusinessModel, TaskResponse>();

        // Configure mapping from TaskBusinessModel to TaskSummaryResponse
        config.NewConfig<TaskBusinessModel, TaskSummaryResponse>();

        // Configure mapping from from TaskCreateRequest to TaskSaveCommand
        config.NewConfig<TaskCreateRequest, TaskSaveCommand>()
            .Map(dest => dest.Id, src => (Guid?)null);

        // Configure mapping from TaskUpdateRequest to TaskSaveCommand
        config.NewConfig<TaskUpdateRequest, TaskSaveCommand>();
    }
}
