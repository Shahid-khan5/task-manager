using System.ComponentModel;
using ModelContextProtocol.Server;
using TaskManagement.Core.Services;
using TaskManagement.Core.Models;

namespace TaskManagement.McpServer.Tools;

[McpServerToolType]
public class ProjectTools
{
    private readonly IProjectService _projectService;

    public ProjectTools(IProjectService projectService)
    {
        _projectService = projectService;
    }

    [McpServerTool]
    [Description("Creates a new project with the specified name and optional description.")]
    public async Task<string> CreateProject(
        [Description("Name of the project")] string name,
        [Description("Optional description of the project")] string? description = null)
    {
        try
        {
            var project = new Project
            {
                Name = name,
                Description = description
            };
            var created = await _projectService.CreateAsync(project);
            return $"Project created successfully: {created.Name} (ID: {created.Id})";
        }
        catch (Exception ex)
        {
            return $"Error creating project: {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Lists all projects in the system.")]
    public async Task<string> ListProjects()
    {
        try
        {
            var projects = await _projectService.GetAllAsync();
            if (!projects.Any())
            {
                return "No projects found.";
            }

            var projectList = string.Join("\n", projects.Select(p =>
                $"- ID: {p.Id}, Name: {p.Name}, Tasks: {p.Tasks?.Count ?? 0}, Created: {p.CreatedAt:yyyy-MM-dd}"));

            return $"Projects:\n{projectList}";
        }
        catch (Exception ex)
        {
            return $"Error listing projects: {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Gets detailed information about a specific project by ID.")]
    public async Task<string> GetProject(
        [Description("ID of the project to retrieve")] int projectId)
    {
        try
        {
            var project = await _projectService.GetByIdAsync(projectId);
            if (project == null)
            {
                return $"Project with ID {projectId} not found.";
            }

            var tasks = project.Tasks?.Select(t =>
                $"  - [{t.Id}] {t.Title} (Status: {t.Status}, Completion: {t.CompletionPercentage}%)") ?? Array.Empty<string>();

            return $"""
                Project Details:
                ID: {project.Id}
                Name: {project.Name}
                Description: {project.Description ?? "N/A"}
                Created: {project.CreatedAt:yyyy-MM-dd HH:mm}
                Tasks ({project.Tasks?.Count ?? 0}):
                {(tasks.Any() ? string.Join("\n", tasks) : "  No tasks")}
                """;
        }
        catch (Exception ex)
        {
            return $"Error retrieving project: {ex.Message}";
        }
    }
    [McpServerTool]
    [Description("Updates an existing project's name and/or description.")]
    public async Task<string> UpdateProject(
        [Description("ID of the project to update")] int projectId,
        [Description("New name for the project (optional)")] string? name = null,
        [Description("New description for the project (optional)")] string? description = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(name) && description == null)
            {
                return "At least one of name or description must be provided.";
            }

            var project = await _projectService.GetByIdAsync(projectId);
            if (project == null)
            {
                return $"Project with ID {projectId} not found.";
            }

            if (!string.IsNullOrWhiteSpace(name))
                project.Name = name;
            if (description != null)
                project.Description = description;

            await _projectService.UpdateAsync(project);
            return $"Project {projectId} updated successfully.";
        }
        catch (Exception ex)
        {
            return $"Error updating project: {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Deletes a project and all its associated tasks.")]
    public async Task<string> DeleteProject(
        [Description("ID of the project to delete")] int projectId)
    {
        try
        {
            var project = await _projectService.GetByIdAsync(projectId);
            if (project == null)
            {
                return $"Project with ID {projectId} not found.";
            }

            var deleted = await _projectService.DeleteAsync(projectId);
            if (deleted)
                return $"Project {projectId} '{project.Name}' deleted successfully.";
            else
                return $"Failed to delete project {projectId}.";
        }
        catch (Exception ex)
        {
            return $"Error deleting project: {ex.Message}";
        }
    }
}
