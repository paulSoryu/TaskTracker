using FluentResults;
using WebApiTaskTracker.Business.Models.Tasks;

namespace WebApiTaskTracker.Business.Services.Tasks;

public interface ITaskService
{
    Task<IReadOnlyCollection<TaskBusinessModel>> GetAllAsync(GetTasksQuery query);
    Task<Result<TaskBusinessModel>> GetByIdAsync(Guid id);
    Task<Result<TaskBusinessModel>> CreateAsync(TaskSaveCommand dto, Guid userId);
    Task<Result> UpdateAsync(TaskSaveCommand task);
    Task<Result> DeleteAsync(Guid id);
}