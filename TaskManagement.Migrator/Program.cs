using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskManagement.Core.Data;

namespace TaskManagement.Migrator
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Task Management Database Migrator");
            Console.WriteLine("==================================");

            // Get database path from args or use default in AppData
            string dbPath;
            if (args.Length > 0)
            {
                dbPath = args[0];
            }
            else
            {
                string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string appFolder = Path.Combine(appDataPath, "TaskManagement");
                Directory.CreateDirectory(appFolder);
                dbPath = Path.Combine(appFolder, "taskmanagement.db");
            }
            string fullDbPath = Path.GetFullPath(dbPath);

            Console.WriteLine($"Database path: {fullDbPath}");

            // Setup services
            var services = new ServiceCollection();
            services.AddDbContext<TaskManagementDbContext>(options =>
                options.UseSqlite($"Data Source={fullDbPath}"));

            var serviceProvider = services.BuildServiceProvider();

            using (var scope = serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TaskManagementDbContext>();

                try
                {
                    Console.WriteLine("Applying migrations...");
                    await dbContext.Database.MigrateAsync();
                    Console.WriteLine("Migrations applied successfully!");

                    // Display current migration status
                    var appliedMigrations = await dbContext.Database.GetAppliedMigrationsAsync();
                    Console.WriteLine($"\nApplied migrations ({appliedMigrations.Count()}):");
                    foreach (var migration in appliedMigrations)
                    {
                        Console.WriteLine($"  - {migration}");
                    }

                    var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
                    if (pendingMigrations.Any())
                    {
                        Console.WriteLine($"\nPending migrations ({pendingMigrations.Count()}):");
                        foreach (var migration in pendingMigrations)
                        {
                            Console.WriteLine($"  - {migration}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("\nNo pending migrations.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error applying migrations: {ex.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                    Environment.Exit(1);
                }
            }

            Console.WriteLine("\nDone!");
        }
    }
}
