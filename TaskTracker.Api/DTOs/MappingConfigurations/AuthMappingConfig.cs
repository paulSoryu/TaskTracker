using Mapster;
using TaskTracker.Api.DTOs.Auths;
using TaskTracker.Business.Models.Auths;

namespace TaskTracker.Api.DTOs.MappingConfigurations;

public class AuthMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // Configure mapping from UserInfoView to UserInfoResponse
        config.NewConfig<UserInfoView, UserInfoResponse>();
    }
}