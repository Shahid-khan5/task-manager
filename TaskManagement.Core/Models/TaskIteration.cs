using System;

namespace TaskManagement.Core.Models
{
    public enum IterationOutcome
    {
        Success,
        Failed,
        Partial,
        Blocked
    }

    /// <summary>
    /// Tracks each attempt/iteration of working on a task
    /// Records what was tried, outcome, and lessons learned
    /// </summary>
    public class TaskIteration
    {
        public int Id { get; set; }
        public int TaskId { get; set; }

        /// <summary>
        /// Sequential iteration number (1, 2, 3, etc.)
        /// </summary>
        public int IterationNumber { get; set; }

        /// <summary>
        /// Description of what approach was taken in this iteration
        /// </summary>
        public string WhatWasTried { get; set; } = string.Empty;

        /// <summary>
        /// Result of this iteration
        /// </summary>
        public IterationOutcome Outcome { get; set; }

        /// <summary>
        /// What was learned - what worked, what didn't, why
        /// This helps future AI agents avoid same mistakes
        /// </summary>
        public string? LessonsLearned { get; set; }

        /// <summary>
        /// Files that were modified in this specific iteration
        /// </summary>
        public string? FilesModified { get; set; }

        public DateTime CreatedAt { get; set; }

        // Navigation
        public TaskItem Task { get; set; } = null!;
    }
}
