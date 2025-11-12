using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Core.Data;
using TaskManagement.Core.Models;
using TaskStatus = TaskManagement.Core.Models.TaskStatus;

namespace TaskManagement.Core.Services
{
    public class TaskService : ITaskService
    {
        private readonly TaskManagementDbContext _context;

        public TaskService(TaskManagementDbContext context)
        {
            _context = context;
        }

        public async Task<TaskItem?> GetByIdAsync(int id)
        {
            return await _context.Tasks
                .Include(t => t.ModifiedFiles)
                .Include(t => t.Issues)
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<IEnumerable<TaskItem>> GetAllAsync()
        {
            return await _context.Tasks
                .Include(t => t.ModifiedFiles)
                .Include(t => t.Issues)
                .Include(t => t.Project)
                .ToListAsync();
        }

        public async Task<TaskItem> CreateAsync(TaskItem task)
        {
            task.CreatedAt = DateTime.UtcNow;
            task.UpdatedAt = DateTime.UtcNow;

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            return task;
        }

        public async Task<TaskItem?> UpdateAsync(TaskItem task)
        {
            var existing = await _context.Tasks.FindAsync(task.Id);
            if (existing == null)
                return null;

            existing.Title = task.Title;
            existing.Description = task.Description;
            existing.Status = task.Status;
            existing.CompletionPercentage = task.CompletionPercentage;
            existing.UpdatedAt = DateTime.UtcNow;

            if (task.Status == TaskStatus.Completed && existing.Status != TaskStatus.Completed)
            {
                existing.CompletedAt = DateTime.UtcNow;
                existing.CompletionPercentage = 100;
            }

            await _context.SaveChangesAsync();

            return existing;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
                return false;

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UpdateCompletionPercentageAsync(int id, int percentage)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
                return false;

            task.CompletionPercentage = Math.Clamp(percentage, 0, 100);
            task.UpdatedAt = DateTime.UtcNow;

            if (task.CompletionPercentage == 100 && task.Status != TaskStatus.Completed)
            {
                task.Status = TaskStatus.Completed;
                task.CompletedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UpdateStatusAsync(int id, TaskStatus status)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
                return false;

            task.Status = status;
            task.UpdatedAt = DateTime.UtcNow;

            if (status == TaskStatus.Completed)
            {
                task.CompletedAt = DateTime.UtcNow;
                task.CompletionPercentage = 100;
            }

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<TaskFile>> GetTaskFilesAsync(int taskId)
        {
            return await _context.TaskFiles
                .Where(f => f.TaskId == taskId)
                .OrderByDescending(f => f.ModifiedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<TaskIssue>> GetTaskIssuesAsync(int taskId)
        {
            return await _context.TaskIssues
                .Where(i => i.TaskId == taskId)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();
        }

        public async Task<TaskFile> AddFileAsync(int taskId, TaskFile file)
        {
            file.TaskId = taskId;
            file.ModifiedAt = DateTime.UtcNow;

            _context.TaskFiles.Add(file);

            // Update task's UpdatedAt timestamp
            var task = await _context.Tasks.FindAsync(taskId);
            if (task != null)
            {
                task.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return file;
        }

        public async Task<TaskIssue> AddIssueAsync(int taskId, TaskIssue issue)
        {
            issue.TaskId = taskId;
            issue.CreatedAt = DateTime.UtcNow;

            _context.TaskIssues.Add(issue);

            // Update task's UpdatedAt timestamp and potentially set status to Blocked
            var task = await _context.Tasks.FindAsync(taskId);
            if (task != null)
            {
                task.UpdatedAt = DateTime.UtcNow;
                if (issue.Status == IssueStatus.Open && task.Status != TaskStatus.Completed)
                {
                    task.Status = TaskStatus.Blocked;
                }
            }

            await _context.SaveChangesAsync();

            return issue;
        }

        public async Task<IEnumerable<TaskItem>> GetSubTasksAsync(int taskId)
        {
            return await _context.Tasks
                .Where(t => t.ParentTaskId == taskId)
                .Include(t => t.ModifiedFiles)
                .Include(t => t.Issues)
                .ToListAsync();
        }

        public async Task<TaskItem> CreateSubTaskAsync(int parentTaskId, TaskItem subTask)
        {
            var parentTask = await _context.Tasks.FindAsync(parentTaskId);
            if (parentTask == null)
                throw new InvalidOperationException($"Parent task with ID {parentTaskId} not found");

            subTask.ParentTaskId = parentTaskId;
            subTask.ProjectId = parentTask.ProjectId;
            subTask.CreatedAt = DateTime.UtcNow;
            subTask.UpdatedAt = DateTime.UtcNow;

            _context.Tasks.Add(subTask);
            await _context.SaveChangesAsync();

            return subTask;
        }

        public async Task<IEnumerable<TaskDependency>> GetTaskDependenciesAsync(int taskId)
        {
            return await _context.TaskDependencies
                .Where(d => d.DependentTaskId == taskId)
                .Include(d => d.DependsOnTask)
                .ToListAsync();
        }

        public async Task<TaskDependency> AddDependencyAsync(int dependentTaskId, int dependsOnTaskId)
        {
            if (dependentTaskId == dependsOnTaskId)
                throw new InvalidOperationException("A task cannot depend on itself");

            // Check for circular dependencies
            if (await HasCircularDependencyAsync(dependentTaskId, dependsOnTaskId))
                throw new InvalidOperationException("This dependency would create a circular dependency");

            var dependency = new TaskDependency
            {
                DependentTaskId = dependentTaskId,
                DependsOnTaskId = dependsOnTaskId,
                CreatedAt = DateTime.UtcNow
            };

            _context.TaskDependencies.Add(dependency);
            await _context.SaveChangesAsync();

            return dependency;
        }

        public async Task<bool> RemoveDependencyAsync(int dependencyId)
        {
            var dependency = await _context.TaskDependencies.FindAsync(dependencyId);
            if (dependency == null)
                return false;

            _context.TaskDependencies.Remove(dependency);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> CanStartTaskAsync(int taskId)
        {
            var dependencies = await _context.TaskDependencies
                .Where(d => d.DependentTaskId == taskId)
                .Include(d => d.DependsOnTask)
                .ToListAsync();

            // All dependencies must be completed
            return dependencies.All(d => d.DependsOnTask.Status == TaskStatus.Completed);
        }

        private async Task<bool> HasCircularDependencyAsync(int dependentTaskId, int dependsOnTaskId)
        {
            // Check if dependsOnTaskId (the task we want to depend on)
            // itself depends on dependentTaskId (directly or indirectly)
            var visited = new HashSet<int>();
            var queue = new Queue<int>();
            queue.Enqueue(dependsOnTaskId);

            while (queue.Count > 0)
            {
                var currentTaskId = queue.Dequeue();
                if (visited.Contains(currentTaskId))
                    continue;

                visited.Add(currentTaskId);

                if (currentTaskId == dependentTaskId)
                    return true; // Circular dependency detected

                var nextDependencies = await _context.TaskDependencies
                    .Where(d => d.DependentTaskId == currentTaskId)
                    .Select(d => d.DependsOnTaskId)
                    .ToListAsync();

                foreach (var depId in nextDependencies)
                {
                    queue.Enqueue(depId);
                }
            }

            return false;
        }
    }
}
