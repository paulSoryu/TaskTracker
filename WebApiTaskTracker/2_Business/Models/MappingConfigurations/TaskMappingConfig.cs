using Mapster;
using WebApiTaskTracker.Business.Models.Tasks;
using WebApiTaskTracker.DataAccess.Entities;

namespace WebApiTaskTracker.Business.Models.MappingConfigurations;

public class TaskMappingConfig : IRegister 
{
    public void Register(TypeAdapterConfig config)
    {
        // Configure mapping from TaskEntity to TaskBusinessModel
        config.NewConfig<TaskEntity, TaskBusinessModel>()
            .RequireDestinationMemberSource(true);

        // Configure mapping from TaskCreateRequest to TaskEntity
        config.NewConfig<TaskSaveCommand, TaskEntity>();
    }
}
