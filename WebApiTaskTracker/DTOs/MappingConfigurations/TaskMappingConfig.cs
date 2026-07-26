using Mapster;
using WebApiTaskTracker.Data.Entities;
using WebApiTaskTracker.DTOs.Tasks;

namespace WebApiTaskTracker.DTOs.MappingConfigurations;

public class TaskMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // Configure mapping from TaskEntity to TaskResponse
        config.NewConfig<TaskEntity, TaskResponse>()
            .RequireDestinationMemberSource(true);

        // Configure mapping from TaskEntity to TaskSummaryResponse
        config.NewConfig<TaskEntity, TaskSummaryResponse>()
            .RequireDestinationMemberSource(true);

        // Configure mapping from TaskCreateRequest to TaskEntity
        config.NewConfig<TaskCreateRequest, TaskEntity>();
            //.IgnoreNonMapped(true);

        // Configure mapping from TaskUpdateRequest to TaskEntity
        config.NewConfig<TaskUpdateRequest, TaskEntity>();
            //.IgnoreNonMapped(true);
    }
}
