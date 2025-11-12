using System;

namespace TaskManagement.Core.Models
{
    public class TaskDependency
    {
        public int Id { get; set; }
        public int DependentTaskId { get; set; } // The task that depends on another
        public int DependsOnTaskId { get; set; } // The task that must be completed first
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public TaskItem DependentTask { get; set; } = null!; // The task that has the dependency
        public TaskItem DependsOnTask { get; set; } = null!; // The task that is depended upon
    }
}
