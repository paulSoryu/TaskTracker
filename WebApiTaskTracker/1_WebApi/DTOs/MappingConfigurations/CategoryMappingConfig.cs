using Mapster;
using WebApiTaskTracker.Business.Models.Categories;
using WebApiTaskTracker.WebApi.DTOs.Categories;

namespace WebApiTaskTracker.WebApi.DTOs.MappingConfigurations;

public class CategoryMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // Configure mapping from CategoryView to CategoryResponse
        config.NewConfig<CategoryView, CategoryResponse>();
            

        // Configure mapping from CategoryView to CategorySummaryResponse
        config.NewConfig<CategoryView, CategoryListResponse>()
            .Map(dest => dest.TaskCount, src => src.Tasks.Count)
            .Map(dest => dest.CompletedTaskCount, src => src.Tasks.Count(t => t.IsCompleted));

        // Configure mapping from CategoryCreateRequest to SaveCategoryCommand
        config.NewConfig<CreateCategoryRequest, SaveCategoryCommand>()
            .Map(dest => dest.Id, src => (Guid?)null);

        // Configure mapping from CategoryUpdateRequest to SaveCategoryCommand
        config.NewConfig<UpdateCategoryRequest, SaveCategoryCommand>();
    }
}
