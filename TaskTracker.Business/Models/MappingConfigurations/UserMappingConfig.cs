using Mapster;
using TaskTracker.Business.Models.Auths;
using TaskTracker.Business.Models.Users;
using TaskTracker.DataAccess.Entities;

namespace TaskTracker.Business.Models.MappingConfigurations;

public class UserMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // Configure mapping from UserEntity to UserInfoView
        config.NewConfig<UserEntity, UserInfoView>()
            .RequireDestinationMemberSource(true)
            .Ignore(dest => dest.IsAdmin); // Couldn't make it work, thus we will load IsAdmin directly in the services

        // Configure mapping from UserEntity to UserView
        config.NewConfig<UserEntity, UserView>()
            .RequireDestinationMemberSource(true)
            .Ignore(dest => dest.IsAdmin); // Couldn't make it work, thus we will load IsAdmin directly in the services
    }
}