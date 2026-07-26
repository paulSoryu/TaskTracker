using WebApiTaskTracker.WebApi.DTOs.Tasks;

namespace WebApiTaskTracker.Business.Services.Tasks;

public interface ITaskService
{
    Task<TaskResponse> GetByIdAsync(Guid id);
    Task<IEnumerable<TaskSummaryResponse>> GetAllAsync();
    Task<TaskResponse> CreateAsync(TaskCreateRequest task, Guid userId);
    Task UpdateAsync(Guid id, TaskUpdateRequest task);
    Task DeleteAsync(Guid id);
}
