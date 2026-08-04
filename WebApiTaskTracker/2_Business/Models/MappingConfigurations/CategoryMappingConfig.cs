using Mapster;
using WebApiTaskTracker.Business.Models.Categories;
using WebApiTaskTracker.DataAccess.Entities;

namespace WebApiTaskTracker.Business.Models.MappingConfigurations;

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
