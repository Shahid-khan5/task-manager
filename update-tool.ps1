#!/usr/bin/env pwsh
# Update Task Management MCP Server
# Run this script after making code changes to update the global tool

Write-Host "🔄 Updating Task Management MCP Server..." -ForegroundColor Cyan

# Step 1: Uninstall the current version
Write-Host "`n📦 Uninstalling current version..." -ForegroundColor Yellow
dotnet tool uninstall --global TaskManagement.McpServer

# Step 2: Increment version (optional - edit the .csproj if you want version tracking)
# You can manually update the version in TaskManagement.McpServer.csproj

# Step 3: Pack the new version
Write-Host "`n📦 Building new package..." -ForegroundColor Yellow
dotnet pack TaskManagement.McpServer/TaskManagement.McpServer.csproj -c Release -o ./nupkg

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Build failed!" -ForegroundColor Red
    exit 1
}

# Step 4: Install the new version
Write-Host "`n✅ Installing new version..." -ForegroundColor Yellow
dotnet tool install --global --add-source ./nupkg TaskManagement.McpServer

if ($LASTEXITCODE -eq 0) {
    Write-Host "`n✨ Successfully updated! The 'taskmcp' command is now using the latest version." -ForegroundColor Green
    Write-Host "`n⚠️  Note: You may need to restart VS Code for changes to take effect." -ForegroundColor Yellow
}
else {
    Write-Host "`n❌ Installation failed!" -ForegroundColor Red
    exit 1
}
