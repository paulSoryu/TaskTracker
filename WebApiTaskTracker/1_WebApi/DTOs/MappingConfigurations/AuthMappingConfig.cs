using Mapster;
using WebApiTaskTracker.Business.Models.Auths;
using WebApiTaskTracker.WebApi.DTOs.Auths;

namespace WebApiTaskTracker.WebApi.DTOs.MappingConfigurations;

public class AuthMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // Configure mapping from UserInfoView to UserInfoResponse
        config.NewConfig<UserInfoView, UserInfoResponse>();
    }
}