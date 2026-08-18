using Mapster;
using TaskTracker.Business.Models.Tasks;
using TaskTracker.DataAccess.Entities;

namespace TaskTracker.Business.Models.MappingConfigurations;

public class TaskMappingConfig : IRegister 
{
    public void Register(TypeAdapterConfig config)
    {
        // Configure mapping from TaskEntity to TaskView
        config.NewConfig<TaskEntity, TaskView>()
              .RequireDestinationMemberSource(true);

        // Configure mapping from SaveTaskCommand to TaskEntity
        config.NewConfig<SaveTaskCommand, TaskEntity>();
    }
}
