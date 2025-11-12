using System;
using System.Collections.Generic;

namespace TaskManagement.Core.Models
{
    public enum TaskStatus
    {
        NotStarted,
        InProgress,
        Completed,
        Blocked
    }

    public class TaskItem
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public int? ParentTaskId { get; set; } // For subtasks
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public TaskStatus Status { get; set; }
        public int CompletionPercentage { get; set; } // 0-100
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        // Navigation properties
        public Project Project { get; set; } = null!;
        public TaskItem? ParentTask { get; set; } // Parent task for subtasks
        public ICollection<TaskItem> SubTasks { get; set; } = new List<TaskItem>();
        public ICollection<TaskFile> ModifiedFiles { get; set; } = new List<TaskFile>();
        public ICollection<TaskIssue> Issues { get; set; } = new List<TaskIssue>();

        // Task dependencies
        public ICollection<TaskDependency> Dependencies { get; set; } = new List<TaskDependency>(); // Tasks that this task depends on
        public ICollection<TaskDependency> Dependents { get; set; } = new List<TaskDependency>(); // Tasks that depend on this task
    }
}
