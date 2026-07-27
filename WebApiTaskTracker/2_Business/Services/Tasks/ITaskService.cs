using WebApiTaskTracker.Business.Models.Tasks;

namespace WebApiTaskTracker.Business.Services.Tasks;

public interface ITaskService
{
    Task<TaskBusinessModel> GetByIdAsync(Guid id);
    Task<IReadOnlyCollection<TaskBusinessModel>> GetAllAsync();
    Task<TaskBusinessModel> CreateAsync(TaskSaveCommand task, Guid userId);
    Task UpdateAsync(TaskSaveCommand task);
    Task DeleteAsync(Guid id);
}
