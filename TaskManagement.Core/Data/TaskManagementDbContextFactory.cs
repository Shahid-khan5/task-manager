using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TaskManagement.Core.Data
{
    public class TaskManagementDbContextFactory : IDesignTimeDbContextFactory<TaskManagementDbContext>
    {
        public TaskManagementDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<TaskManagementDbContext>();

            // Use a temporary in-memory database for design-time operations
            // The actual database path will be configured at runtime
            optionsBuilder.UseSqlite("Data Source=:memory:");

            return new TaskManagementDbContext(optionsBuilder.Options);
        }
    }
}
