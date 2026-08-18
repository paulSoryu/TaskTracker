using Mapster;
using TaskTracker.Api.DTOs.Categories;
using TaskTracker.Business.Models.Categories;

namespace TaskTracker.Api.DTOs.MappingConfigurations;

public class CategoryMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // Configure mapping from CategoryView to CategoryResponse
        config.NewConfig<CategoryView, CategoryResponse>();


        // Configure mapping from CategoryView to CategoryListResponse
        config.NewConfig<CategoryView, CategoryListResponse>()
            .Map(dest => dest.TaskCount, src => src.Tasks.Count)
            .Map(dest => dest.CompletedTaskCount, src => src.Tasks.Count(t => t.IsCompleted));

        // Configure mapping from CreateCategoryRequest to SaveCategoryCommand
        config.NewConfig<CreateCategoryRequest, SaveCategoryCommand>()
            .Map(dest => dest.Id, src => (Guid?)null);

        // Configure mapping from UpdateCategoryRequest to SaveCategoryCommand
        config.NewConfig<UpdateCategoryRequest, SaveCategoryCommand>();
    }
}
