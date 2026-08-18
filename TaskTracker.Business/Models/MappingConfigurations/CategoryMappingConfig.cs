using Mapster;
using TaskTracker.Business.Models.Categories;
using TaskTracker.DataAccess.Entities;

namespace TaskTracker.Business.Models.MappingConfigurations;

public class CategoryMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // Configure mapping from CategoryEntity to CategoryBusinessModel
        config.NewConfig<CategoryEntity, CategoryView>()
            .RequireDestinationMemberSource(true);

        // Configure mapping from SaveCategoryCommand to CategoryEntity
        config.NewConfig<SaveCategoryCommand, CategoryEntity>();
    }
}
