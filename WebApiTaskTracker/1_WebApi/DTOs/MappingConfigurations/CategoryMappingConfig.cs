using Mapster;
using WebApiTaskTracker.DataAccess.Entities;
using WebApiTaskTracker.WebApi.DTOs.Categories;

namespace WebApiTaskTracker.WebApi.DTOs.MappingConfigurations;

public class CategoryMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // Configure mapping from CategoryEntity to CategoryResponse
        config.NewConfig<CategoryEntity, CategoryResponse>()
            .RequireDestinationMemberSource(true);

        // Configure mapping from CategoryEntity to CategorySummaryResponse
        config.NewConfig<CategoryEntity, CategorySummaryResponse>()
            .RequireDestinationMemberSource(true)
            .Map(dest => dest.TaskCount, src => src.Tasks.Count);

        // Configure mapping from CategoryCreateRequest to CategoryEntity
        config.NewConfig<CategoryCreateRequest, CategoryEntity>();
            //.IgnoreNonMapped(true);

        // Configure mapping from CategoryUpdateRequest to CategoryEntity
        config.NewConfig<CategoryUpdateRequest, CategoryEntity>();
            //.IgnoreNonMapped(true);
    }
}
