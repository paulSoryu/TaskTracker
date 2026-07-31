using FluentResults;
using WebApiTaskTracker.Business.Models.Tasks;

namespace WebApiTaskTracker.Business.Services.Tasks;

public interface ITaskService
{
    Task<Result<TaskBusinessModel>> GetByIdAsync(Guid id);
    // Retrieving all tasks generally doesn't return an error, thus the return type doesn't need to be wrapped in a Result.
    Task<IReadOnlyCollection<TaskBusinessModel>> GetAllAsync(GetTasksQuery query);
    Task<Result<TaskBusinessModel>> CreateAsync(TaskSaveCommand dto, Guid userId);
    Task<Result> UpdateAsync(TaskSaveCommand task);
    Task<Result> DeleteAsync(Guid id);
}