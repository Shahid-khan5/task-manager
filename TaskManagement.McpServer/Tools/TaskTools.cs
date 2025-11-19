using System.ComponentModel;
using ModelContextProtocol.Server;
using TaskManagement.Core.Services;
using TaskManagement.Core.Models;
using TaskStatus = TaskManagement.Core.Models.TaskStatus;

namespace TaskManagement.McpServer.Tools;

[McpServerToolType]
public class TaskTools
{
    private readonly ITaskService _taskService;

    public TaskTools(ITaskService taskService)
    {
        _taskService = taskService;
    }

    [McpServerTool]
    [Description("Creates a new task with specified details.")]
    public async Task<string> CreateTask(
        [Description("Title of the task")] string title,
        [Description("Optional description of the task")] string? description = null,
        [Description("Status (NotStarted=0, InProgress=1, Completed=2, Blocked=3)")] int status = 0)
    {
        try
        {
            var task = new TaskItem
            {
                Title = title,
                Description = description,
                Status = (TaskStatus)status,
                CompletionPercentage = 0
            };

            var created = await _taskService.CreateAsync(task);
            return $"Task created successfully: {created.Title} (ID: {created.Id})";
        }
        catch (Exception ex)
        {
            return $"Error creating task: {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Lists all tasks or filters tasks by status.")]
    public async Task<string> ListTasks(
        [Description("Optional status filter (NotStarted=0, InProgress=1, Completed=2, Blocked=3). If null, returns all tasks.")] int? status = null)
    {
        try
        {
            var allTasks = status.HasValue
                ? await _taskService.GetByStatusAsync((TaskStatus)status.Value)
                : await _taskService.GetByStatusAsync(null);

            if (!allTasks.Any())
            {
                return status.HasValue
                    ? $"No tasks found with status: {(TaskStatus)status.Value}."
                    : "No tasks found.";
            }

            var taskList = string.Join("\n", allTasks.Select(t =>
                $"- [{t.Id}] {t.Title} | Status: {t.Status} | Completion: {t.CompletionPercentage}%"));

            var header = status.HasValue
                ? $"Tasks with status '{(TaskStatus)status.Value}':"
                : "All Tasks:";

            return $"{header}\n{taskList}";
        }
        catch (Exception ex)
        {
            return $"Error listing tasks: {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Gets detailed information about a specific task.")]
    public async Task<string> GetTask(
        [Description("ID of the task to retrieve")] int taskId)
    {
        try
        {
            var task = await _taskService.GetByIdAsync(taskId);
            if (task == null)
            {
                return $"Task with ID {taskId} not found.";
            }

            var dependencies = task.Dependencies?.Select(d =>
                $"  - Depends on Task {d.DependsOnTaskId}") ?? Array.Empty<string>();

            var files = task.ModifiedFiles?.Select(f =>
                $"  - {f.FilePath} (Modified: {f.ModifiedAt:yyyy-MM-dd})") ?? Array.Empty<string>();

            var issues = task.Issues?.Select(i =>
                $"  - [{i.Status}] {i.Title}") ?? Array.Empty<string>();

            return $"""
                Task Details:
                ID: {task.Id}
                Title: {task.Title}
                Description: {task.Description ?? "N/A"}
                Status: {task.Status}
                Completion: {task.CompletionPercentage}%
                Created: {task.CreatedAt:yyyy-MM-dd HH:mm}
                Completed: {task.CompletedAt?.ToString("yyyy-MM-dd HH:mm") ?? "N/A"}
                Parent Task ID: {task.ParentTaskId?.ToString() ?? "N/A"}
                
                Dependencies ({task.Dependencies?.Count ?? 0}):
                {(dependencies.Any() ? string.Join("\n", dependencies) : "  None")}
                
                Modified Files ({task.ModifiedFiles?.Count ?? 0}):
                {(files.Any() ? string.Join("\n", files) : "  None")}
                
                Issues ({task.Issues?.Count ?? 0}):
                {(issues.Any() ? string.Join("\n", issues) : "  None")}
                """;
        }
        catch (Exception ex)
        {
            return $"Error retrieving task: {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Updates task status only. Use this to mark tasks as InProgress, Completed, etc. without modifying other fields.")]
    public async Task<string> UpdateTaskStatus(
        [Description("ID of the task to update")] int taskId,
        [Description("New status (NotStarted=0, InProgress=1, Completed=2, Blocked=3)")] int status)
    {
        try
        {
            var success = await _taskService.UpdateStatusAsync(taskId, (TaskStatus)status);
            if (!success)
            {
                return $"Task with ID {taskId} not found.";
            }

            return $"Task {taskId} status updated to {(TaskStatus)status}.";
        }
        catch (Exception ex)
        {
            return $"Error updating task status: {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Updates task title, description, or completion percentage. Does NOT update status - use UpdateTaskStatus for that.")]
    public async Task<string> UpdateTask(
        [Description("ID of the task to update")] int taskId,
        [Description("New title (optional)")] string? title = null,
        [Description("New description (optional)")] string? description = null,
        [Description("New completion percentage 0-100 (optional)")] int? completionPercentage = null)
    {
        try
        {
            var task = await _taskService.GetByIdAsync(taskId);
            if (task == null)
            {
                return $"Task with ID {taskId} not found.";
            }

            if (!string.IsNullOrWhiteSpace(title))
                task.Title = title;
            if (description != null)
                task.Description = description;
            if (completionPercentage.HasValue)
                task.CompletionPercentage = Math.Clamp(completionPercentage.Value, 0, 100);

            await _taskService.UpdateAsync(task);
            return $"Task {taskId} updated successfully.";
        }
        catch (Exception ex)
        {
            return $"Error updating task: {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Deletes a task and all its associated data.")]
    public async Task<string> DeleteTask(
        [Description("ID of the task to delete")] int taskId)
    {
        try
        {
            var task = await _taskService.GetByIdAsync(taskId);
            if (task == null)
            {
                return $"Task with ID {taskId} not found.";
            }

            var deleted = await _taskService.DeleteAsync(taskId);
            if (deleted)
                return $"Task {taskId} '{task.Title}' deleted successfully.";
            else
                return $"Failed to delete task {taskId}.";
        }
        catch (Exception ex)
        {
            return $"Error deleting task: {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Adds a dependency between two tasks.")]
    public async Task<string> AddTaskDependency(
        [Description("ID of the task that depends on another")] int dependentTaskId,
        [Description("ID of the task that must be completed first")] int dependsOnTaskId)
    {
        try
        {
            await _taskService.AddDependencyAsync(dependentTaskId, dependsOnTaskId);
            return $"Dependency added: Task {dependentTaskId} depends on Task {dependsOnTaskId}";
        }
        catch (Exception ex)
        {
            return $"Error adding dependency: {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Adds a file modification record to a task.")]
    public async Task<string> CreateSubTask(
        [Description("ID of the parent task")] int parentTaskId,
        [Description("Title of the subtask")] string title,
        [Description("Optional description")] string? description = null)
    {
        try
        {
            var subTask = new TaskItem
            {
                Title = title,
                Description = description,
                Status = TaskStatus.NotStarted,
                CompletionPercentage = 0
            };
            var created = await _taskService.CreateSubTaskAsync(parentTaskId, subTask);
            return $"Subtask created: {created.Title} (ID: {created.Id}) under Task {parentTaskId}";
        }
        catch (Exception ex)
        {
            return $"Error creating subtask: {ex.Message}";
        }
    }
}
