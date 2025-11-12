using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskManagement.Core.Models;

namespace TaskManagement.Core.Services
{
    public interface IProjectService
    {
        Task<Project?> GetByIdAsync(int id);
        Task<IEnumerable<Project>> GetAllAsync();
        Task<Project> CreateAsync(Project project);
        Task<Project?> UpdateAsync(Project project);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<TaskItem>> GetProjectTasksAsync(int projectId);
    }
}
