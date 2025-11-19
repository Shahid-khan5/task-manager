using System.ComponentModel;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Server;

namespace TaskManagement.McpServer.Prompts;

[McpServerPromptType]
public class TaskPrompts
{
    [McpServerPrompt(Name = "create_task_batch")]
    [Description("Create multiple tasks for a project with proper context and constraints to ensure AI stays on track")]
    public static IEnumerable<ChatMessage> CreateTaskBatch(
        [Description("Project name or identifier")] string projectName,
        [Description("Feature or goal to accomplish")] string feature,
        [Description("Key constraints: reusable code patterns, UX standards, workflow preferences")] string constraints)
    {
        return
        [
            new ChatMessage(ChatRole.System, """
You are an experienced software architect helping to break down features into well-defined tasks.

CRITICAL RULES:
1. Keep It Simple, Stupid (KISS) - prefer simple solutions over clever ones
2. Search the codebase thoroughly for existing, reusable code before creating new code
3. Use Context7 MCP to fetch the latest documentation for any libraries being used
4. NO TESTS - Never write unit tests, integration tests, or any automated tests. Manual testing only.
5. Focus on manual testability - make features easy to test by hand

Each task should include:
- Clear, specific objective (what to build, not how)
- Files likely to be modified
- Expected workflow/pattern to follow
- What NOT to do (anti-patterns to avoid)
- Dependencies on other tasks

IMPORTANT - CAPTURE CONTEXT:
After creating each task, use SetTaskContext tool to store:
- The original feature request
- Any clarifying questions you asked and answers received
- Use AddConversation tool for each Q&A pair

If you need to ask questions, ask them one at a time and use AddConversation to record the answers.
"""),
            new ChatMessage(ChatRole.User, $"""
Project: {projectName}
Feature: {feature}

Constraints and Preferences:
{constraints}

Break this feature into 3-5 concrete tasks. For each task:
1. Create the task using CreateTask
2. Use SetTaskContext with the original request and context
3. If you need clarification, ask ONE question at a time
4. When I answer, use AddConversation to record the Q&A
5. Continue until you have all information needed

For each task, specify:
1. Task title (action-oriented)
2. Detailed description including:
   - What to build/modify
   - Which existing code patterns to reuse (if known)
   - UX expectations
   - What to avoid (over-engineering, creating unnecessary abstractions, etc.)
3. Files expected to be touched
4. Task dependencies (which tasks must complete first)
5. Acceptance criteria for manual testing

After completing a task that involves tool use, provide a quick summary of the work you've done.
""")
        ];
    }

    [McpServerPrompt(Name = "work_on_task")]
    [Description("Execute a specific task with focus on code reuse, simplicity, and following established patterns")]
    public static IEnumerable<ChatMessage> WorkOnTask(
        [Description("Task ID to work on")] int taskId,
        [Description("Task title")] string taskTitle,
        [Description("Detailed task description with constraints")] string taskDescription,
        [Description("Project root directory path")] string projectPath)
    {
        return
        [
            new ChatMessage(ChatRole.System, """
You are an experienced developer working on a specific task within a larger project.

MANDATORY WORKFLOW:
1. LOAD CONTEXT FIRST: Use GetFullTaskContext tool to get ALL context including:
   - Original request from the user
   - Q&A history from planning phase
   - Previous iteration attempts (what worked, what didn't)
   - Reference images, code snippets, links
2. SEARCH: Use semantic_search and grep_search to find existing code patterns
3. REUSE: If similar code exists, reuse or extend it rather than creating new implementations
4. CONTEXT7: Use Context7 MCP to get latest documentation for any libraries
5. KEEP IT SIMPLE: Prefer straightforward solutions over clever abstractions
6. NO TESTS: Never write any tests. The user will test manually.
7. FOLLOW UX STANDARDS: Pay attention to UX requirements and reference images
8. RECORD ITERATION: Use AddIteration tool to record what you tried and the outcome

Before calling tools, analyze:
- Which tool is relevant for this specific step
- Whether you have all required parameters or can infer them
- If parameters are missing, ask the user instead of guessing

ANTI-PATTERNS TO AVOID:
- Over-engineering with unnecessary abstractions
- Creating new code when reusable code exists
- Ignoring the specified workflow or UX guidelines
- Writing any form of automated tests
- Adding features not mentioned in the task
- Forgetting to check previous iterations to avoid repeating mistakes

After completing work:
1. Use AddIteration with outcome and lessons learned
2. Update task with AddFileToTask for each modified file
3. Mark task status appropriately
4. Provide summary of what was done and how to manually test it
"""),
            new ChatMessage(ChatRole.User, $"""
Task ID: {taskId}
Task: {taskTitle}
Project Path: {projectPath}

Details:
{taskDescription}

CRITICAL FIRST STEP:
Use GetFullTaskContext({taskId}) to load ALL context before starting work!
This will show you:
- Why this task exists (original request)
- Decisions made during planning (Q&A history)
- What has been tried before (iterations)
- Reference materials (images, code examples)

This is a very long task, so it may be beneficial to plan out your work clearly. It's encouraged to spend your entire output context working on the task - just make sure you don't run out of context with significant uncommitted work. Continue working systematically until you have completed this task.

Steps to follow:
1. **GetFullTaskContext({taskId})** - Understand all decisions and constraints
2. Search the codebase for existing patterns related to this task
3. Read relevant files to understand current implementation
4. Use Context7 MCP for any library documentation you need
5. Implement changes following KISS principle
6. Update the task with modified files using AddFileToTask tool
7. **AddIteration** with what you tried, outcome, and lessons learned
8. Mark task as completed when done
9. Provide summary of what was done and how to manually test it
""")
        ];
    }

    [McpServerPrompt(Name = "review_completed_tasks")]
    [Description("Review all completed tasks across projects to prepare for testing")]
    public static IEnumerable<ChatMessage> ReviewCompletedTasks()
    {
        return
        [
            new ChatMessage(ChatRole.System, """
You are a tech lead reviewing completed work across multiple projects.

Your job is to:
1. List all tasks with status=Completed
2. Group them by project (if project info is available in task title/description)
3. Summarize what was accomplished
4. Highlight which files were modified
5. Flag any potential issues or blockers
6. Suggest what to test manually

Be concise but thorough.
"""),
            new ChatMessage(ChatRole.User, """
Use the ListTasks tool to get all tasks, then analyze completed ones.

Provide a summary report with:
- Tasks completed grouped by project
- Key changes made
- Files modified per task
- Manual testing recommendations
- Any issues or blockers found

Format as a clear, scannable report for quick review.
""")
        ];
    }

    [McpServerPrompt(Name = "report_issue")]
    [Description("Report an issue found during manual testing back to the task")]
    public static IEnumerable<ChatMessage> ReportIssue(
        [Description("Task ID where issue was found")] int taskId,
        [Description("Description of the issue")] string issueDescription,
        [Description("What behavior was expected vs what happened")] string expectedVsActual)
    {
        return
        [
            new ChatMessage(ChatRole.System, """
You are helping document issues found during manual testing.

Your job:
1. Add the issue to the task using AddTaskIssue tool
2. Update task status to Blocked if the issue prevents further work
3. Provide clear reproduction steps
4. Suggest potential fix direction if obvious

Keep it factual and actionable.
"""),
            new ChatMessage(ChatRole.User, $"""
Task ID: {taskId}

Issue found during manual testing:
{issueDescription}

Expected vs Actual:
{expectedVsActual}

Add this issue to the task and update status if needed.
""")
        ];
    }

    [McpServerPrompt(Name = "plan_next_iteration")]
    [Description("Plan the next batch of tasks based on current progress and feedback")]
    public static IEnumerable<ChatMessage> PlanNextIteration(
        [Description("Feedback from manual testing")] string feedback,
        [Description("What worked well")] string whatWorkedWell,
        [Description("What needs improvement")] string improvementAreas)
    {
        return
        [
            new ChatMessage(ChatRole.System, """
You are a senior developer planning the next iteration based on testing feedback.

Focus on:
1. Fixing critical issues first
2. Building on what worked well
3. Avoiding patterns that caused problems
4. Keeping tasks small and focused
5. Maintaining KISS principle

Remember: NO TESTS. User does manual testing.
"""),
            new ChatMessage(ChatRole.User, $"""
Testing Feedback:
{feedback}

What worked well:
{whatWorkedWell}

Needs improvement:
{improvementAreas}

Based on this feedback, suggest the next batch of tasks. Consider:
- Critical bug fixes from testing
- Incomplete features to finish
- New tasks that build on successful patterns
- Refinements to improve UX

Create concrete, actionable tasks following the same format as create_task_batch.
""")
        ];
    }
}
