using System;

namespace TaskManagement.Core.Models
{
    public enum ReferenceType
    {
        Image,          // Screenshot, mockup, diagram
        File,           // Reference code file
        Url,            // External link
        CodeSnippet,    // Inline code example
        Note            // Text note/reminder
    }

    /// <summary>
    /// Stores references attached to a task (images, files, links, code snippets)
    /// Images are stored in filesystem and path is stored here
    /// </summary>
    public class TaskReference
    {
        public int Id { get; set; }
        public int TaskId { get; set; }

        public ReferenceType ReferenceType { get; set; }

        /// <summary>
        /// For Image/File: relative path from database folder
        /// For Url: the URL
        /// For CodeSnippet/Note: the actual content
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Description of what this reference is for
        /// E.g., "Expected UX mockup", "Pattern to follow from UserList.tsx"
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// For images: original filename
        /// </summary>
        public string? OriginalFileName { get; set; }

        /// <summary>
        /// For images: mime type (image/png, image/jpeg, etc.)
        /// </summary>
        public string? MimeType { get; set; }

        public DateTime CreatedAt { get; set; }

        // Navigation
        public TaskItem Task { get; set; } = null!;
    }
}
