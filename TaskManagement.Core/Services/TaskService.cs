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
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<IEnumerable<TaskItem>> GetAllAsync()
        {
            return await _context.Tasks
                .Include(t => t.ModifiedFiles)
                .Include(t => t.Issues)
                .ToListAsync();
        }

        public async Task<IEnumerable<TaskItem>> GetByStatusAsync(TaskStatus? status = null)
        {
            var query = _context.Tasks
                .Include(t => t.ModifiedFiles)
                .Include(t => t.Issues)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(t => t.Status == status.Value);
            }

            return await query.ToListAsync();
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

        // Context management
        public async Task<TaskContext?> GetTaskContextAsync(int taskId)
        {
            return await _context.TaskContexts
                .FirstOrDefaultAsync(c => c.TaskId == taskId);
        }

        public async Task<TaskContext> SetTaskContextAsync(int taskId, string originalRequest, string? conversationHistory = null, string? additionalNotes = null)
        {
            var existingContext = await GetTaskContextAsync(taskId);
            if (existingContext != null)
            {
                throw new InvalidOperationException($"Task {taskId} already has context. Use UpdateTaskContextAsync to modify it.");
            }

            var context = new TaskContext
            {
                TaskId = taskId,
                OriginalRequest = originalRequest,
                ConversationHistory = conversationHistory,
                AdditionalNotes = additionalNotes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.TaskContexts.Add(context);
            await _context.SaveChangesAsync();
            return context;
        }

        public async Task<TaskContext> UpdateTaskContextAsync(int taskId, string? originalRequest = null, string? conversationHistory = null, string? additionalNotes = null)
        {
            var context = await GetTaskContextAsync(taskId);
            if (context == null)
            {
                throw new InvalidOperationException($"Task {taskId} does not have context. Use SetTaskContextAsync first.");
            }

            if (originalRequest != null)
                context.OriginalRequest = originalRequest;

            if (conversationHistory != null)
                context.ConversationHistory = conversationHistory;

            if (additionalNotes != null)
                context.AdditionalNotes = additionalNotes;

            context.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return context;
        }

        public async Task<bool> AddConversationAsync(int taskId, string question, string answer)
        {
            var context = await GetTaskContextAsync(taskId);
            if (context == null)
            {
                return false;
            }

            var conversations = System.Text.Json.JsonSerializer.Deserialize<List<Dictionary<string, string>>>(context.ConversationHistory ?? "[]")
                ?? new List<Dictionary<string, string>>();

            conversations.Add(new Dictionary<string, string>
            {
                ["question"] = question,
                ["answer"] = answer,
                ["timestamp"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
            });

            context.ConversationHistory = System.Text.Json.JsonSerializer.Serialize(conversations);
            context.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        // Iteration management
        public async Task<IEnumerable<TaskIteration>> GetTaskIterationsAsync(int taskId)
        {
            return await _context.TaskIterations
                .Where(i => i.TaskId == taskId)
                .OrderBy(i => i.IterationNumber)
                .ToListAsync();
        }

        public async Task<TaskIteration> AddIterationAsync(int taskId, string whatWasTried, IterationOutcome outcome, string? lessonsLearned = null, string? filesModified = null)
        {
            var task = await _context.Tasks.FindAsync(taskId);
            if (task == null)
            {
                throw new InvalidOperationException($"Task {taskId} not found");
            }

            var existingIterations = await _context.TaskIterations
                .Where(i => i.TaskId == taskId)
                .ToListAsync();

            var nextIterationNumber = existingIterations.Any()
                ? existingIterations.Max(i => i.IterationNumber) + 1
                : 1;

            var iteration = new TaskIteration
            {
                TaskId = taskId,
                IterationNumber = nextIterationNumber,
                WhatWasTried = whatWasTried,
                Outcome = outcome,
                LessonsLearned = lessonsLearned,
                FilesModified = filesModified,
                CreatedAt = DateTime.UtcNow
            };

            _context.TaskIterations.Add(iteration);
            await _context.SaveChangesAsync();
            return iteration;
        }

        // Reference management
        public async Task<IEnumerable<TaskReference>> GetTaskReferencesAsync(int taskId)
        {
            return await _context.TaskReferences
                .Where(r => r.TaskId == taskId)
                .OrderBy(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<TaskReference> AddReferenceAsync(int taskId, ReferenceType type, string content, string description, string? originalFileName = null, string? mimeType = null)
        {
            var task = await _context.Tasks.FindAsync(taskId);
            if (task == null)
            {
                throw new InvalidOperationException($"Task {taskId} not found");
            }

            var reference = new TaskReference
            {
                TaskId = taskId,
                ReferenceType = type,
                Content = content,
                Description = description,
                OriginalFileName = originalFileName,
                MimeType = mimeType,
                CreatedAt = DateTime.UtcNow
            };

            _context.TaskReferences.Add(reference);
            await _context.SaveChangesAsync();
            return reference;
        }

        public async Task<bool> DeleteReferenceAsync(int referenceId)
        {
            var reference = await _context.TaskReferences.FindAsync(referenceId);
            if (reference == null)
            {
                return false;
            }

            _context.TaskReferences.Remove(reference);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
