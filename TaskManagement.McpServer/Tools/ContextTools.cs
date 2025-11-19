using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;
using TaskManagement.Core.Models;
using TaskManagement.Core.Services;

namespace TaskManagement.McpServer.Tools;

[McpServerToolType]
public class ContextTools
{
    private readonly ITaskService _taskService;
    private readonly string _dbPath;

    public ContextTools(ITaskService taskService)
    {
        _taskService = taskService;
        // Get db path from environment or use default
        _dbPath = Environment.GetEnvironmentVariable("TASK_DB_PATH")
            ?? Path.Combine(Directory.GetCurrentDirectory(), ".taskmanagement.db");
    }

    [McpServerTool]
    [Description("Set the original request/context for a task. Use this when creating tasks to capture the initial requirements.")]
    public async Task<string> SetTaskContext(
        [Description("ID of the task")] int taskId,
        [Description("The original feature request or requirement from the user")] string originalRequest,
        [Description("Optional additional notes or context")] string? additionalNotes = null)
    {
        try
        {
            var context = await _taskService.SetTaskContextAsync(taskId, originalRequest, null, additionalNotes);
            return $"Context set for task {taskId}. Original request captured.";
        }
        catch (Exception ex)
        {
            return $"Error setting context: {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Add a Q&A pair to the task's conversation history. Use when AI asks questions and user provides answers during planning.")]
    public async Task<string> AddConversation(
        [Description("ID of the task")] int taskId,
        [Description("The question that was asked")] string question,
        [Description("The answer provided by the user")] string answer)
    {
        try
        {
            var success = await _taskService.AddConversationAsync(taskId, question, answer);
            if (success)
            {
                return $"Conversation added to task {taskId}.";
            }
            else
            {
                // If context doesn't exist, create it first
                await _taskService.SetTaskContextAsync(taskId, "Context created during Q&A");
                await _taskService.AddConversationAsync(taskId, question, answer);
                return $"Context created and conversation added to task {taskId}.";
            }
        }
        catch (Exception ex)
        {
            return $"Error adding conversation: {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Get the full context of a task including original request and conversation history. Use this before working on a task to understand all decisions made.")]
    public async Task<string> GetTaskContext(
        [Description("ID of the task")] int taskId)
    {
        try
        {
            var context = await _taskService.GetTaskContextAsync(taskId);
            if (context == null)
            {
                return $"No context found for task {taskId}. Use SetTaskContext to add context.";
            }

            var result = $@"Task {taskId} Context:

=== ORIGINAL REQUEST ===
{context.OriginalRequest}

";

            if (!string.IsNullOrEmpty(context.ConversationHistory))
            {
                result += "=== CONVERSATION HISTORY ===\n";
                var conversations = JsonSerializer.Deserialize<List<Dictionary<string, string>>>(context.ConversationHistory);
                if (conversations != null)
                {
                    foreach (var conv in conversations)
                    {
                        result += $"\n[{conv.GetValueOrDefault("timestamp", "N/A")}]\n";
                        result += $"Q: {conv.GetValueOrDefault("question", "")}\n";
                        result += $"A: {conv.GetValueOrDefault("answer", "")}\n";
                    }
                }
                result += "\n";
            }

            if (!string.IsNullOrEmpty(context.AdditionalNotes))
            {
                result += $@"=== ADDITIONAL NOTES ===
{context.AdditionalNotes}

";
            }

            return result;
        }
        catch (Exception ex)
        {
            return $"Error getting context: {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Record an iteration/attempt of working on a task. Use this to track what was tried and what the outcome was.")]
    public async Task<string> AddIteration(
        [Description("ID of the task")] int taskId,
        [Description("Description of what approach was taken in this iteration")] string whatWasTried,
        [Description("Outcome: Success=0, Failed=1, Partial=2, Blocked=3")] int outcome,
        [Description("What was learned - what worked, what didn't, why")] string? lessonsLearned = null,
        [Description("Comma-separated list of files modified in this iteration")] string? filesModified = null)
    {
        try
        {
            var iteration = await _taskService.AddIterationAsync(
                taskId,
                whatWasTried,
                (IterationOutcome)outcome,
                lessonsLearned,
                filesModified);

            return $"Iteration {iteration.IterationNumber} recorded for task {taskId}. Outcome: {iteration.Outcome}";
        }
        catch (Exception ex)
        {
            return $"Error adding iteration: {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Add an image reference to a task. Supports base64 data, file path, or URL. The image will be copied to task attachments.")]
    public async Task<string> AddImageReference(
        [Description("ID of the task")] int taskId,
        [Description("Image source: base64 encoded data, local file path, or HTTP/HTTPS URL")] string imageSource,
        [Description("Description of what this image shows (e.g., 'Expected UX mockup', 'Current bug screenshot')")] string description,
        [Description("Original filename (optional, auto-detected for paths/URLs)")] string? fileName = null,
        [Description("MIME type (optional, auto-detected)")] string? mimeType = null)
    {
        try
        {
            byte[] imageBytes;
            string originalFileName = fileName ?? "image.png";
            string detectedMimeType = mimeType ?? "image/png";

            // Determine source type and get image bytes
            if (imageSource.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                imageSource.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                // Download from URL
                using var httpClient = new HttpClient();
                imageBytes = await httpClient.GetByteArrayAsync(imageSource);

                // Try to get filename from URL
                if (string.IsNullOrEmpty(fileName))
                {
                    var uri = new Uri(imageSource);
                    originalFileName = Path.GetFileName(uri.LocalPath);
                    if (string.IsNullOrEmpty(originalFileName) || originalFileName == "/")
                    {
                        originalFileName = "downloaded_image.png";
                    }
                }

                // Detect MIME type from response or extension
                if (string.IsNullOrEmpty(mimeType))
                {
                    var ext = Path.GetExtension(originalFileName).ToLowerInvariant();
                    detectedMimeType = ext switch
                    {
                        ".jpg" or ".jpeg" => "image/jpeg",
                        ".png" => "image/png",
                        ".gif" => "image/gif",
                        ".webp" => "image/webp",
                        ".svg" => "image/svg+xml",
                        _ => "image/png"
                    };
                }
            }
            else if (File.Exists(imageSource))
            {
                // Read from local file
                imageBytes = await File.ReadAllBytesAsync(imageSource);

                if (string.IsNullOrEmpty(fileName))
                {
                    originalFileName = Path.GetFileName(imageSource);
                }

                if (string.IsNullOrEmpty(mimeType))
                {
                    var ext = Path.GetExtension(imageSource).ToLowerInvariant();
                    detectedMimeType = ext switch
                    {
                        ".jpg" or ".jpeg" => "image/jpeg",
                        ".png" => "image/png",
                        ".gif" => "image/gif",
                        ".webp" => "image/webp",
                        ".svg" => "image/svg+xml",
                        _ => "image/png"
                    };
                }
            }
            else
            {
                // Assume base64
                try
                {
                    imageBytes = Convert.FromBase64String(imageSource);
                }
                catch
                {
                    return $"Error: Could not process image source. Please provide valid base64 data, file path, or URL.";
                }
            }

            // Create attachments directory if it doesn't exist
            var dbDir = Path.GetDirectoryName(_dbPath) ?? Directory.GetCurrentDirectory();
            var attachmentsDir = Path.Combine(dbDir, "task_attachments", taskId.ToString());
            Directory.CreateDirectory(attachmentsDir);

            // Generate unique filename
            var extension = Path.GetExtension(originalFileName);
            if (string.IsNullOrEmpty(extension))
            {
                extension = detectedMimeType switch
                {
                    "image/jpeg" => ".jpg",
                    "image/png" => ".png",
                    "image/gif" => ".gif",
                    "image/webp" => ".webp",
                    "image/svg+xml" => ".svg",
                    _ => ".png"
                };
            }

            var uniqueFileName = $"{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid():N}{extension}";
            var filePath = Path.Combine(attachmentsDir, uniqueFileName);

            // Save image
            await File.WriteAllBytesAsync(filePath, imageBytes);

            // Store relative path from db directory
            var relativePath = Path.Combine("task_attachments", taskId.ToString(), uniqueFileName);

            var reference = await _taskService.AddReferenceAsync(
                taskId,
                ReferenceType.Image,
                relativePath,
                description,
                originalFileName,
                detectedMimeType);

            return $"Image '{originalFileName}' saved to {relativePath} and referenced in task {taskId}. Size: {imageBytes.Length / 1024}KB";
        }
        catch (Exception ex)
        {
            return $"Error adding image reference: {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Get COMPLETE context for a task: original request, Q&A history, iterations, and all references. Use before working on any task!")]
    public async Task<string> GetFullTaskContext(
        [Description("ID of the task")] int taskId)
    {
        try
        {
            var result = "";

            // Get basic context
            var contextResult = await GetTaskContext(taskId);
            result += contextResult + "\n";

            // Get iterations
            var iterations = await _taskService.GetTaskIterationsAsync(taskId);
            if (iterations.Any())
            {
                result += "=== PREVIOUS ITERATIONS ===\n";
                foreach (var iteration in iterations)
                {
                    result += $"\nIteration {iteration.IterationNumber} ({iteration.Outcome}) - {iteration.CreatedAt:yyyy-MM-dd HH:mm}\n";
                    result += $"Tried: {iteration.WhatWasTried}\n";
                    if (!string.IsNullOrEmpty(iteration.LessonsLearned))
                    {
                        result += $"Learned: {iteration.LessonsLearned}\n";
                    }
                }
                result += "\n";
            }

            // Get references
            var references = await _taskService.GetTaskReferencesAsync(taskId);
            if (references.Any())
            {
                result += "=== REFERENCES ===\n";
                foreach (var reference in references)
                {
                    result += $"\n[{reference.ReferenceType}] {reference.Description}\n";
                    if (reference.ReferenceType == ReferenceType.Image)
                    {
                        var dbDir = Path.GetDirectoryName(_dbPath) ?? Directory.GetCurrentDirectory();
                        var fullPath = Path.Combine(dbDir, reference.Content);
                        result += $"Image file: {fullPath}\n";
                    }
                    else
                    {
                        result += $"Content: {reference.Content}\n";
                    }
                }
                result += "\n";
            }

            return result;
        }
        catch (Exception ex)
        {
            return $"Error getting full context: {ex.Message}";
        }
    }
}
