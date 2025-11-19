#!/usr/bin/env pwsh
# Install Task Management MCP Server
# Run this script to install the MCP server as a global .NET tool

Write-Host "╔══════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║  Task Management MCP Server - Installation               ║" -ForegroundColor Cyan
Write-Host "╚══════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

# Check if .NET 10 is installed
Write-Host "🔍 Checking .NET installation..." -ForegroundColor Yellow
$dotnetVersion = dotnet --version 2>$null
if (-not $dotnetVersion) {
    Write-Host "❌ .NET is not installed!" -ForegroundColor Red
    Write-Host "Please install .NET 10 SDK from: https://dotnet.microsoft.com/download" -ForegroundColor Yellow
    exit 1
}
Write-Host "✅ .NET $dotnetVersion detected" -ForegroundColor Green

# Step 1: Build and pack the tool
Write-Host "`n📦 Building package..." -ForegroundColor Yellow
dotnet pack TaskManagement.McpServer/TaskManagement.McpServer.csproj -c Release -o ./nupkg

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Build failed!" -ForegroundColor Red
    exit 1
}

# Step 2: Uninstall existing version (if any)
Write-Host "`n🗑️  Removing any existing installation..." -ForegroundColor Yellow
dotnet tool uninstall --global TaskManagement.McpServer 2>$null
# Don't check exit code since it's ok if tool wasn't installed

# Step 3: Install the tool globally
Write-Host "`n📥 Installing globally..." -ForegroundColor Yellow
dotnet tool install --global --add-source ./nupkg TaskManagement.McpServer

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Installation failed!" -ForegroundColor Red
    exit 1
}

# Step 4: Run migrations to set up database
Write-Host "`n🗄️  Setting up database..." -ForegroundColor Yellow
dotnet run --project TaskManagement.Migrator/TaskManagement.Migrator.csproj

if ($LASTEXITCODE -ne 0) {
    Write-Host "⚠️  Database setup had issues, but continuing..." -ForegroundColor Yellow
}

Write-Host "`n╔══════════════════════════════════════════════════════════╗" -ForegroundColor Green
Write-Host "║  ✅ Installation Complete!                               ║" -ForegroundColor Green
Write-Host "╚══════════════════════════════════════════════════════════╝" -ForegroundColor Green
Write-Host ""
Write-Host "📝 Next steps:" -ForegroundColor Cyan
Write-Host "  1. Add this to your project's .vscode/mcp.json:" -ForegroundColor White
Write-Host ""
Write-Host '     {' -ForegroundColor Gray
Write-Host '       "servers": {' -ForegroundColor Gray
Write-Host '         "TaskManagementMcp": {' -ForegroundColor Gray
Write-Host '           "type": "stdio",' -ForegroundColor Gray
Write-Host '           "command": "taskmcp",' -ForegroundColor Gray
Write-Host '           "args": ["--db=.taskmanagement.db"]' -ForegroundColor Gray
Write-Host '         }' -ForegroundColor Gray
Write-Host '       }' -ForegroundColor Gray
Write-Host '     }' -ForegroundColor Gray
Write-Host ""
Write-Host "  2. Restart VS Code" -ForegroundColor White
Write-Host "  3. Ask GitHub Copilot to create tasks!" -ForegroundColor White
Write-Host ""
Write-Host "💡 The 'taskmcp' command is now available globally" -ForegroundColor Cyan
Write-Host ""
