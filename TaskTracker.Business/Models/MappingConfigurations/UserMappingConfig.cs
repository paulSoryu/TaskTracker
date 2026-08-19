using Mapster;
using TaskTracker.Business.Models.Auths;
using TaskTracker.DataAccess.Entities;

namespace TaskTracker.Business.Models.MappingConfigurations;

public class UserMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // Configure mapping from UserEntity to UserInfoView
        config.NewConfig<UserEntity, UserInfoView>()
            .RequireDestinationMemberSource(true);
    }
}