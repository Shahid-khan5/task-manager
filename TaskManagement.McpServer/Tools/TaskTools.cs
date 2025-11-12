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
    [Description("Creates a new task in a project with specified details.")]
    public async Task<string> CreateTask(
        [Description("ID of the project to add the task to")] int projectId,
        [Description("Title of the task")] string title,
        [Description("Optional description of the task")] string? description = null,
        [Description("Status (NotStarted=0, InProgress=1, Completed=2, Blocked=3)")] int status = 0)
    {
        try
        {
            var task = new TaskItem
            {
                ProjectId = projectId,
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
    [Description("Lists all tasks in a specific project.")]
    public async Task<string> ListTasksInProject(
        [Description("ID of the project")] int projectId)
    {
        try
        {
            var allTasks = await _taskService.GetAllAsync();
            var tasks = allTasks.Where(t => t.ProjectId == projectId).ToList();

            if (!tasks.Any())
            {
                return $"No tasks found in project {projectId}.";
            }

            var taskList = string.Join("\n", tasks.Select(t =>
                $"- [{t.Id}] {t.Title} | Status: {t.Status} | Completion: {t.CompletionPercentage}%"));

            return $"Tasks in Project {projectId}:\n{taskList}";
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
                Project ID: {task.ProjectId}
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
    [Description("Updates an existing task's properties.")]
    public async Task<string> UpdateTask(
        [Description("ID of the task to update")] int taskId,
        [Description("New title (optional)")] string? title = null,
        [Description("New description (optional)")] string? description = null,
        [Description("New status (NotStarted=0, InProgress=1, Completed=2, Blocked=3, optional)")] int? status = null,
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
            if (status.HasValue)
                task.Status = (TaskStatus)status.Value;
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
    [Description("Removes a dependency by its ID.")]
    public async Task<string> RemoveTaskDependency(
        [Description("ID of the dependency to remove")] int dependencyId)
    {
        try
        {
            var removed = await _taskService.RemoveDependencyAsync(dependencyId);
            if (removed)
                return $"Dependency {dependencyId} removed successfully.";
            else
                return $"Dependency {dependencyId} not found.";
        }
        catch (Exception ex)
        {
            return $"Error removing dependency: {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Adds a file modification record to a task.")]
    public async Task<string> AddFileToTask(
        [Description("ID of the task")] int taskId,
        [Description("File path")] string filePath,
        [Description("Optional description of changes")] string? changeDescription = null)
    {
        try
        {
            var file = new TaskFile
            {
                TaskId = taskId,
                FilePath = filePath,
                ChangeDescription = changeDescription
            };
            await _taskService.AddFileAsync(taskId, file);
            return $"File '{filePath}' added to Task {taskId}";
        }
        catch (Exception ex)
        {
            return $"Error adding file: {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Adds an issue/blocker to a task.")]
    public async Task<string> AddTaskIssue(
        [Description("ID of the task")] int taskId,
        [Description("Title of the issue")] string title,
        [Description("Optional description of the issue")] string? description = null)
    {
        try
        {
            var issue = new TaskIssue
            {
                TaskId = taskId,
                Title = title,
                Description = description,
                Status = IssueStatus.Open
            };
            await _taskService.AddIssueAsync(taskId, issue);
            return $"Issue '{title}' added to Task {taskId}";
        }
        catch (Exception ex)
        {
            return $"Error adding issue: {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Creates a subtask under a parent task.")]
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

    [McpServerTool]
    [Description("Checks if a task can be started based on its dependencies.")]
    public async Task<string> CanStartTask(
        [Description("ID of the task to check")] int taskId)
    {
        try
        {
            var canStart = await _taskService.CanStartTaskAsync(taskId);
            if (canStart)
                return $"Task {taskId} can be started - all dependencies are completed.";
            else
                return $"Task {taskId} cannot be started yet - some dependencies are not completed.";
        }
        catch (Exception ex)
        {
            return $"Error checking task: {ex.Message}";
        }
    }
}
