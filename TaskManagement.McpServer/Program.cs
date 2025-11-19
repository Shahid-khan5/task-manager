using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using ModelContextProtocol.Server;
using TaskManagement.Core.Data;
using TaskManagement.Core.Services;
using TaskManagement.McpServer.Tools;
using TaskManagement.McpServer.Prompts;

// Build the host application
HostApplicationBuilder builder = Host.CreateEmptyApplicationBuilder(settings: null);

// Get database path - always use .project-management folder in user's home directory
string dbPath;
if (args.Length > 0 && args[0].StartsWith("--db="))
{
    dbPath = args[0].Substring(5);
}
else
{
    // Default to .project-management folder in user's home directory
    var homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    var projectManagementDir = Path.Combine(homeDirectory, ".project-management");
    Directory.CreateDirectory(projectManagementDir);
    dbPath = Path.Combine(projectManagementDir, "taskmanagement.db");
}

// Ensure directory exists
var dbDirectory = Path.GetDirectoryName(dbPath);
if (!string.IsNullOrEmpty(dbDirectory))
{
    Directory.CreateDirectory(dbDirectory);
}

// Configure database context
var connectionString = $"Data Source={dbPath}";


builder.Services.AddDbContext<TaskManagementDbContext>(options =>
    options.UseSqlite(connectionString));

// Register services
builder.Services.AddScoped<ITaskService, TaskService>();

// Register MCP server, tools, and prompts
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly()
    .WithPrompts<TaskPrompts>();

// Build and run
using var host = builder.Build();

// Ensure database is created
using (var scope = host.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<TaskManagementDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

await host.RunAsync();
