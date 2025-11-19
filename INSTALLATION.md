# Task Management MCP Server - Installation Guide

## Option 1: Install as .NET Global Tool (Recommended)

This makes your MCP server available globally from any directory.

### Step 1: Pack the Tool

```powershell
# Navigate to the project directory
cd "d:\tools\task manager"

# Create the NuGet package
dotnet pack TaskManagement.McpServer/TaskManagement.McpServer.csproj -c Release -o ./nupkg
```

### Step 2: Install Globally

```powershell
# Install from local package
dotnet tool install --global --add-source ./nupkg TaskManagement.McpServer
```

### Step 3: Run the Server

After installation, you can run it from anywhere:

```powershell
taskmcp
```

### Updating After Code Changes

After making changes to your code, run the update script:

```powershell
# Easy way - use the update script
.\update-tool.ps1
```

Or manually:

```powershell
# 1. Uninstall old version
dotnet tool uninstall --global TaskManagement.McpServer

# 2. Rebuild and pack
dotnet pack TaskManagement.McpServer/TaskManagement.McpServer.csproj -c Release -o ./nupkg

# 3. Reinstall
dotnet tool install --global --add-source ./nupkg TaskManagement.McpServer

# 4. Restart VS Code to pick up changes
```

**Important:** After updating, restart VS Code for the MCP server to use the new version.

## Option 2: Use Absolute Path (Current Method)

Keep using your current configuration with the full path to the project.

## Configure for Projects

### Per-Project Database (Recommended)

Create a `.vscode/mcp.json` file in your project root:

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

This creates a `.taskmanagement.db` file in your project directory. Each project has its own isolated task database.

**Benefits:**

- ✅ No need to manage project IDs
- ✅ Tasks are scoped to each project automatically
- ✅ Database lives with your project
- ✅ Easy to version control (add `.taskmanagement.db` to `.gitignore`)

### Global Database (Alternative)

Update `%APPDATA%\Code\User\globalStorage\github.copilot\config\mcp.json`:

```json
{
  "servers": {
    "TaskManagementMcp": {
      "type": "stdio",
      "command": "taskmcp",
      "args": ["--db=C:\\Users\\YourUsername\\taskmanagement.db"]
    }
  }
}
```

Or omit the `--db` argument to use `.taskmanagement.db` in the current directory.

### For Claude Desktop

Update `%APPDATA%\Claude\claude_desktop_config.json`:

```json
{
  "mcpServers": {
    "TaskManagementMcp": {
      "command": "taskmcp",
      "args": ["--db=C:\\Users\\YourUsername\\taskmanagement.db"]
    }
  }
}
```

## Database Location Options

You can specify the database location using the `--db` argument:

```bash
# Project-specific (relative path)
--db=.taskmanagement.db

# Absolute path
--db=C:\path\to\your\project\tasks.db

# If no --db argument is provided, defaults to .taskmanagement.db in current directory
```

**Tip:** Add `.taskmanagement.db` to your `.gitignore` if using per-project databases.

## Running the Migrator

After installation, run migrations to set up the database:

```powershell
cd "d:\tools\task manager"
dotnet run --project TaskManagement.Migrator/TaskManagement.Migrator.csproj
```

## Troubleshooting

### Tool not found after installation

```powershell
# Check if tool is installed
dotnet tool list --global

# Ensure dotnet tools path is in PATH
echo $env:PATH
```

### Database connection issues

- Verify the database file path exists
- Check file permissions
- Ensure the directory is writable

### VS Code not detecting the server

- Restart VS Code after configuration changes
- Check Developer Tools (Help → Toggle Developer Tools) for errors
- Verify the JSON syntax in mcp.json
