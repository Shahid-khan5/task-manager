# Task Management System

A comprehensive task management system designed for AI agents to manage, track, and coordinate tasks across projects. Includes an MCP (Model Context Protocol) server for seamless AI integration.

## Project Structure

```
task manager/
├── TaskManagement.Core/           # Core business logic and data layer
│   ├── Models/                    # Data models
│   │   ├── Project.cs            # Project entity
│   │   ├── TaskItem.cs           # Task entity with subtasks
│   │   ├── TaskFile.cs           # File modification tracking
│   │   ├── TaskIssue.cs          # Issue/blocker tracking
│   │   └── TaskDependency.cs     # Task dependency tracking
│   ├── Data/                      # Database context
│   │   ├── TaskManagementDbContext.cs
│   │   └── TaskManagementDbContextFactory.cs
│   ├── Services/                  # Business logic services
│   │   ├── IProjectService.cs
│   │   ├── ProjectService.cs
│   │   ├── ITaskService.cs
│   │   └── TaskService.cs
│   └── Migrations/                # EF Core migrations
│
├── TaskManagement.Migrator/       # Database migration console app
│   └── Program.cs
│
├── TaskManagement.McpServer/      # MCP server for AI integration
│   ├── Tools/                     # MCP tool implementations
│   │   ├── ProjectTools.cs       # Project management tools
│   │   └── TaskTools.cs          # Task management tools
│   └── Program.cs
│
└── .vscode/
    └── mcp.json                   # GitHub Copilot MCP configuration
```

## Features

### 1. **Projects**

- Create and manage projects
- Each project contains multiple tasks
- Track creation and update timestamps

### 2. **Tasks**

- Full CRUD operations
- **Status tracking**: NotStarted, InProgress, Completed, Blocked
- **Completion percentage**: Track progress from 0-100%
- **Subtasks**: Hierarchical task structure with parent-child relationships
- **Dependencies**: Define task dependencies with circular dependency detection
- **Modified files tracking**: Log all files modified during task work
- **Issue/blocker tracking**: Document problems encountered during task execution

### 3. **Task Files**

- Track which files were modified for each task
- Include change descriptions
- Timestamp when files were modified
- Essential for AI agents to know what was changed

### 4. **Task Issues**

- Document blockers and problems
- Status: Open, Resolved, Deferred
- Link issues to specific tasks
- Track resolution timestamps

### 5. **Task Dependencies**

- Define which tasks depend on others
- Circular dependency detection
- Check if a task can be started based on dependencies
- Automatic status management

## Database

- **SQLite** database stored in: `%APPDATA%\TaskManagement\taskmanagement.db`
- Entity Framework Core 10.0
- Automatic migrations

## Getting Started

### Prerequisites

- .NET 10.0 SDK

### Build the Projects

```bash
# Build the core library
cd TaskManagement.Core
dotnet build

# Build the migrator
cd ../TaskManagement.Migrator
dotnet build
```

### Run Migrations

The migrator will create/update the database automatically:

```bash
cd TaskManagement.Migrator
dotnet run
```

Or specify a custom database path:

```bash
dotnet run "C:\path\to\custom\database.db"
```

## Service Usage Examples

### Project Service

```csharp
// Create a project
var project = await projectService.CreateAsync(new Project
{
    Name = "My Project",
    Description = "Project description"
});

// Get all tasks in a project
var tasks = await projectService.GetProjectTasksAsync(project.Id);
```

### Task Service

```csharp
// Create a task
var task = await taskService.CreateAsync(new TaskItem
{
    ProjectId = projectId,
    Title = "Implement feature X",
    Description = "Detailed description",
    Status = TaskStatus.NotStarted,
    CompletionPercentage = 0
});

// Update completion percentage
await taskService.UpdateCompletionPercentageAsync(task.Id, 50);

// Add a modified file
await taskService.AddFileAsync(task.Id, new TaskFile
{
    FilePath = "src/services/MyService.cs",
    ChangeDescription = "Implemented new method"
});

// Add an issue/blocker
await taskService.AddIssueAsync(task.Id, new TaskIssue
{
    Title = "Missing dependency",
    Description = "Package XYZ is not installed",
    Status = IssueStatus.Open
});

// Create a subtask
var subtask = await taskService.CreateSubTaskAsync(task.Id, new TaskItem
{
    Title = "Subtask 1",
    Status = TaskStatus.NotStarted
});

// Add a task dependency
var dependency = await taskService.AddDependencyAsync(
    dependentTaskId: task2.Id,
    dependsOnTaskId: task1.Id
);

// Check if a task can be started
bool canStart = await taskService.CanStartTaskAsync(task2.Id);
```

## Next Steps

### Phase 2: MCP Server

- Implement Model Context Protocol server
- Expose task management operations via MCP
- Enable AI agents to interact with the system
- Add real-time notifications

### Phase 3: Additional Features

- Task templates
- Task scheduling/due dates
- Task assignments
- Activity logging
- Search and filtering
- Reports and analytics

## Database Schema

### Projects Table

- Id (int, PK)
- Name (string, max 200)
- Description (string, max 1000, nullable)
- CreatedAt (DateTime)
- UpdatedAt (DateTime)

### Tasks Table

- Id (int, PK)
- ProjectId (int, FK)
- ParentTaskId (int, FK, nullable)
- Title (string, max 300)
- Description (string, max 2000, nullable)
- Status (enum: NotStarted, InProgress, Completed, Blocked)
- CompletionPercentage (int, 0-100)
- CreatedAt (DateTime)
- UpdatedAt (DateTime)
- CompletedAt (DateTime, nullable)

### TaskFiles Table

- Id (int, PK)
- TaskId (int, FK)
- FilePath (string, max 500)
- ChangeDescription (string, max 1000, nullable)
- ModifiedAt (DateTime)

### TaskIssues Table

- Id (int, PK)
- TaskId (int, FK)
- Title (string, max 300)
- Description (string, max 2000, nullable)
- Status (enum: Open, Resolved, Deferred)
- CreatedAt (DateTime)
- ResolvedAt (DateTime, nullable)

### TaskDependencies Table

- Id (int, PK)
- DependentTaskId (int, FK) - The task that has the dependency
- DependsOnTaskId (int, FK) - The task that must be completed first
- CreatedAt (DateTime)
- Unique constraint on (DependentTaskId, DependsOnTaskId)

## License

This project is designed for internal use by AI agents for task management and coordination.
