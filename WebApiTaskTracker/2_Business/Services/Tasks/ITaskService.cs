using FluentResults;
using WebApiTaskTracker.Business.Models.Tasks;

namespace WebApiTaskTracker.Business.Services.Tasks;

public interface ITaskService
{
    Task<IReadOnlyCollection<TaskView>> GetAllAsync(GetTasksQuery query);
    Task<Result<TaskView>> GetByIdAsync(Guid id);
    Task<Result<TaskView>> CreateAsync(TaskSaveCommand dto, Guid userId);
    Task<Result> UpdateAsync(TaskSaveCommand task);
    Task<Result> DeleteAsync(Guid id);
}