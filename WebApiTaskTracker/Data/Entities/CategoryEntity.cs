using System.Drawing;
using WebApiTaskTracker.Utilities;

namespace WebApiTaskTracker.Data.Entities;

// This entity can be made more secure by making the setters private and using a constructor to set the properties, but for simplicity, we will keep it as is.
// Private setters would also complicate the mapping with Mapster, which is used in this project for DTO mapping.
public class CategoryEntity
{
    public Guid Id { get; set; }
    public required string Title { get; set; } = null!;
    public string Colour { get; set; } = "#FFFFFF";


    public required Guid UserId { get; set; }
    public UserEntity User { get; set; } = null!;


    public List<TaskEntity> Tasks { get; set; } = [];
}
