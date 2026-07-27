using Mapster;
using WebApiTaskTracker.Business.Models.Categories;
using WebApiTaskTracker.WebApi.DTOs.Categories;

namespace WebApiTaskTracker.WebApi.DTOs.MappingConfigurations;

public class CategoryMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // Configure mapping from CategoryBusinessModel to CategoryResponse
        config.NewConfig<CategoryBusinessModel, CategoryResponse>();
            

        // Configure mapping from CategoryBusinessModel to CategorySummaryResponse
        config.NewConfig<CategoryBusinessModel, CategorySummaryResponse>()
            .Map(dest => dest.TaskCount, src => src.Tasks.Count);

        // Configure mapping from CategoryCreateRequest to CategorySaveCommand
        config.NewConfig<CategoryCreateRequest, CategorySaveCommand>()
            .Map(dest => dest.Id, src => (Guid?)null);

        // Configure mapping from CategoryUpdateRequest to CategorySaveCommand
        config.NewConfig<CategoryUpdateRequest, CategorySaveCommand>();
    }
}
