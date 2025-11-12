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

## Configure for Global Use

### For VS Code

Update `%APPDATA%\Code\User\globalStorage\github.copilot\config\mcp.json`:

```json
{
  "servers": {
    "TaskManagementMcp": {
      "type": "stdio",
      "command": "taskmcp",
      "env": {
        "DB_CONNECTION_STRING": "Data Source=C:\\Users\\YourUsername\\taskmanagement.db"
      }
    }
  }
}
```

### For Claude Desktop

Update `%APPDATA%\Claude\claude_desktop_config.json`:

```json
{
  "mcpServers": {
    "TaskManagementMcp": {
      "command": "taskmcp",
      "env": {
        "DB_CONNECTION_STRING": "Data Source=C:\\Users\\YourUsername\\taskmanagement.db"
      }
    }
  }
}
```

## Database Location

Choose a permanent location for your database:

- User profile: `C:\Users\YourUsername\taskmanagement.db`
- Application data: `%APPDATA%\TaskManagement\taskmanagement.db`
- Custom location of your choice

Update the `DB_CONNECTION_STRING` environment variable accordingly.

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
