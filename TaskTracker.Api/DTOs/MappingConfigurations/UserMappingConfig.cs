using Mapster;
using TaskTracker.Api.DTOs.Users;
using TaskTracker.Business.Models.Users;

namespace TaskTracker.Api.DTOs.MappingConfigurations;

public class UserMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // Configure mapping from UserView to UserInfoResponse
        config.NewConfig<UserView, UserResponse>()
            .Map(dest => dest.TaskCount, src => src.Tasks.Count)
            .Map(dest => dest.CompletedTaskCount, src => src.Tasks.Count(t => t.IsCompleted))
            .Map(dest => dest.CategoryCount, src => src.Categories.Count);

        // Configure mapping from UserView to UserListResponse
        config.NewConfig<UserView, UserListResponse>()
            .Map(dest => dest.TaskCount, src => src.Tasks.Count)
            .Map(dest => dest.CompletedTaskCount, src => src.Tasks.Count(t => t.IsCompleted))
            .Map(dest => dest.CategoryCount, src => src.Categories.Count);
    }
}