using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskManagement.Core.Models;
using TaskStatus = TaskManagement.Core.Models.TaskStatus;

namespace TaskManagement.Core.Services
{
    public interface ITaskService
    {
        Task<TaskItem?> GetByIdAsync(int id);
        Task<IEnumerable<TaskItem>> GetAllAsync();
        Task<TaskItem> CreateAsync(TaskItem task);
        Task<TaskItem?> UpdateAsync(TaskItem task);
        Task<bool> DeleteAsync(int id);
        Task<bool> UpdateCompletionPercentageAsync(int id, int percentage);
        Task<bool> UpdateStatusAsync(int id, TaskStatus status);
        Task<IEnumerable<TaskFile>> GetTaskFilesAsync(int taskId);
        Task<IEnumerable<TaskIssue>> GetTaskIssuesAsync(int taskId);
        Task<TaskFile> AddFileAsync(int taskId, TaskFile file);
        Task<TaskIssue> AddIssueAsync(int taskId, TaskIssue issue);

        // Subtask management
        Task<IEnumerable<TaskItem>> GetSubTasksAsync(int taskId);
        Task<TaskItem> CreateSubTaskAsync(int parentTaskId, TaskItem subTask);

        // Task dependency management
        Task<IEnumerable<TaskDependency>> GetTaskDependenciesAsync(int taskId);
        Task<TaskDependency> AddDependencyAsync(int dependentTaskId, int dependsOnTaskId);
        Task<bool> RemoveDependencyAsync(int dependencyId);
        Task<bool> CanStartTaskAsync(int taskId);
    }
}
