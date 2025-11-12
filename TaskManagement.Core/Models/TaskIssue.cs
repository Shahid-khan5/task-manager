using System;

namespace TaskManagement.Core.Models
{
    public enum IssueStatus
    {
        Open,
        Resolved,
        Deferred
    }

    public class TaskIssue
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public IssueStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }

        // Navigation property
        public TaskItem Task { get; set; } = null!;
    }
}
