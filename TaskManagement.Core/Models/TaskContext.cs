using System;

namespace TaskManagement.Core.Models
{
    /// <summary>
    /// Stores the original request and conversation history for a task
    /// This preserves context so different AI agents can understand decisions made
    /// </summary>
    public class TaskContext
    {
        public int Id { get; set; }
        public int TaskId { get; set; }

        /// <summary>
        /// The original request/feature description from the user
        /// </summary>
        public string OriginalRequest { get; set; } = string.Empty;

        /// <summary>
        /// JSON array of conversation Q&A pairs during task planning
        /// Format: [{"question": "...", "answer": "...", "timestamp": "..."}]
        /// </summary>
        public string? ConversationHistory { get; set; }

        /// <summary>
        /// Additional notes or context about the task
        /// </summary>
        public string? AdditionalNotes { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation
        public TaskItem Task { get; set; } = null!;
    }
}
