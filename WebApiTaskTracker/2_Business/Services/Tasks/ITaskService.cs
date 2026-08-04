using FluentResults;
using WebApiTaskTracker.Business.Models;
using WebApiTaskTracker.Business.Models.Tasks;

namespace WebApiTaskTracker.Business.Services.Tasks;

public interface ITaskService
{
    Task<PagedResult<TaskView>> GetAllAsync(GetTasksQuery query);
    Task<Result<TaskView>> GetByIdAsync(Guid id);
    Task<Result<TaskView>> CreateAsync(SaveTaskCommand dto, Guid userId);
    Task<Result> UpdateAsync(SaveTaskCommand task);
    Task<Result> DeleteAsync(Guid id);
    Task<Result> ReorderTaskAsync(MoveTaskCommand command);
}