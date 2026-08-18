using TaskTracker.DataAccess.Interfaces;
using TaskTracker.Shared.Enums;

namespace TaskTracker.DataAccess.Entities;

// This entity can be made more secure by making the setters private and using a constructor to set the properties, but for simplicity, we will keep it as is.
// Private setters would also complicate the mapping with Mapster, which is used in this project for DTO mapping.
public class TaskEntity : IAuditable, IOrderable
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public DateOnly? DueDate { get; set; }
    public TaskPriority Priority { get; set; }
    public bool IsCompleted { get; set; } = false;
    public DateTime CreatedAt { get; set; }
    public int Position { get; set; }


    public required Guid UserId { get; set; }
    public UserEntity User { get; set; } = null!;

    
    public Guid? CategoryId { get; set; } 
    public CategoryEntity? Category { get; set; }
}