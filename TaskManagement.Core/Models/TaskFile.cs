using System;

namespace TaskManagement.Core.Models
{
    public class TaskFile
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public string? ChangeDescription { get; set; }
        public DateTime ModifiedAt { get; set; }

        // Navigation property
        public TaskItem Task { get; set; } = null!;
    }
}
