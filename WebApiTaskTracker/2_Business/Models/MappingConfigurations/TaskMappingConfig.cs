using Mapster;
using WebApiTaskTracker.Business.Models.Tasks;
using WebApiTaskTracker.DataAccess.Entities;

namespace WebApiTaskTracker.Business.Models.MappingConfigurations;

public class TaskMappingConfig : IRegister 
{
    public void Register(TypeAdapterConfig config)
    {
        // Configure mapping from TaskEntity to TaskView
        config.NewConfig<TaskEntity, TaskView>()
            .RequireDestinationMemberSource(true);

        // Configure mapping from TaskSaveCommand to TaskEntity
        config.NewConfig<TaskSaveCommand, TaskEntity>();
    }
}
