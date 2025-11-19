# Task Management MCP Server - Version History

This file tracks version changes. Update the version in `TaskManagement.McpServer/TaskManagement.McpServer.csproj` to trigger a new release.

## How to Release a New Version

1. Update the version number in `TaskManagement.McpServer/TaskManagement.McpServer.csproj`:

   ```xml
   <Version>1.0.1</Version>
   ```

2. Commit and push to master:

   ```powershell
   git add .
   git commit -m "Release v1.0.1: Description of changes"
   git push origin master
   ```

3. GitHub Actions will automatically:
   - Build the project
   - Create a NuGet package
   - Create a GitHub release with tag
   - Attach the .nupkg file

## Version History

### v1.0.0 (Initial Release)

- Task management with status tracking
- Context and conversation history
- Iteration tracking
- Image references support
- Subtask and dependency management
- 12 optimized MCP tools for AI agents
