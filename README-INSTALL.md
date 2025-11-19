# Task Management MCP Server

A comprehensive task management system designed for AI agents to manage, track, and coordinate tasks. Includes an MCP (Model Context Protocol) server for seamless AI integration with GitHub Copilot and Claude Desktop.

## 🚀 Quick Installation

### Option 1: Download from Releases (Recommended)

1. **Download** the latest `.nupkg` file from [Releases](https://github.com/Shahid-khan5/task-manager/releases)

2. **Install** the tool:
```powershell
# Navigate to your Downloads folder (or wherever you saved the file)
cd ~\Downloads

# Install the tool
dotnet tool install --global --add-source . TaskManagement.McpServer
```

3. **Configure** VS Code - Create `.vscode/mcp.json` in your project:
```json
{
  "servers": {
    "TaskManagementMcp": {
      "type": "stdio",
      "command": "taskmcp",
      "args": ["--db=.taskmanagement.db"]
    }
  }
}
```

4. **Restart VS Code** and start using task management with GitHub Copilot!

### Option 2: Build from Source

```powershell
git clone https://github.com/Shahid-khan5/task-manager.git
cd task-manager
.\install.ps1
```

## 📋 Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download) or later
- VS Code with GitHub Copilot extension (or Claude Desktop)

## 🔄 Updating

```powershell
# Uninstall old version
dotnet tool uninstall --global TaskManagement.McpServer

# Download new .nupkg from releases, then install
dotnet tool install --global --add-source ~\Downloads TaskManagement.McpServer
```

## ✨ Features

### Task Management (12 Optimized Tools)

**Core Operations:**
- `CreateTask` - Create new tasks with title, description, and status
- `ListTasks` - View all tasks or filter by status
- `GetTask` - Get detailed task information
- `UpdateTask` - Update task properties (title, description, completion %)
- `UpdateTaskStatus` - Dedicated status updates (prevents null title bugs)
- `DeleteTask` - Remove tasks

**Advanced Features:**
- `CreateSubTask` - Create subtasks under parent tasks
- `AddTaskDependency` - Define task dependencies

**Context System:**
- `SetTaskContext` - Capture original requirements
- `AddConversation` - Track Q&A during planning
- `GetTaskContext` - Review task context
- `AddIteration` - Record attempts and learnings
- `AddImageReference` - Attach mockups/screenshots
- `GetFullTaskContext` - Complete task history

### Key Capabilities

✅ **Status Tracking** - NotStarted, InProgress, Completed, Blocked  
✅ **Task Dependencies** - Define task relationships  
✅ **Context Preservation** - Keep track of requirements and decisions  
✅ **Iteration History** - Learn from previous attempts  
✅ **Image Attachments** - Link mockups and screenshots  
✅ **Per-Project Databases** - Each project has its own task database  
✅ **AI-Optimized** - Designed for GitHub Copilot and Claude Desktop

## 📚 Usage Examples

**Create a task:**
```
@workspace Create a task to implement user authentication
```

**List tasks:**
```
@workspace Show me all in-progress tasks
```

**Update status:**
```
@workspace Mark task 5 as completed
```

**Add context:**
```
@workspace Add context to task 3: User wants OAuth support with Google and GitHub
```

## 🗄️ Database Configuration

### Per-Project (Recommended)
Each project gets its own `.taskmanagement.db` file:
```json
{
  "servers": {
    "TaskManagementMcp": {
      "type": "stdio",
      "command": "taskmcp",
      "args": ["--db=.taskmanagement.db"]
    }
  }
}
```

### Global Database
Share tasks across all projects:
```json
{
  "servers": {
    "TaskManagementMcp": {
      "type": "stdio",
      "command": "taskmcp",
      "args": ["--db=C:\\Users\\YourName\\tasks.db"]
    }
  }
}
```

## 🛠️ For Claude Desktop

Add to `%APPDATA%\Claude\claude_desktop_config.json`:
```json
{
  "mcpServers": {
    "TaskManagementMcp": {
      "command": "taskmcp",
      "args": ["--db=C:\\Users\\YourName\\tasks.db"]
    }
  }
}
```

## 📖 Documentation

- [Installation Guide](INSTALLATION.md) - Detailed installation instructions
- [Quick Reference](QUICK_REFERENCE.md) - All available tools
- [Changelog](CHANGELOG.md) - Version history

## 🤝 Contributing

Contributions are welcome! Feel free to open issues or submit pull requests.

## 📄 License

MIT License - See [LICENSE](LICENSE) for details

## 👤 Author

**Shahid Khan**  
GitHub: [@Shahid-khan5](https://github.com/Shahid-khan5)

## 🌟 Support

If you find this useful, please star the repository!
