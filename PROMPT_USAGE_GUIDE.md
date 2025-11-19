# Task Management MCP Server - Prompt Usage Guide

## Overview

This MCP server provides **5 specialized prompts** designed to help you work on multiple projects simultaneously while keeping AI agents on track and following your standards.

## Core Philosophy

Based on Anthropic's prompt engineering best practices:

- **KISS (Keep It Simple, Stupid)**: Simple solutions over clever abstractions
- **Code Reuse**: Search and reuse existing code before creating new
- **Context7 Integration**: Always fetch latest library documentation
- **NO TESTS**: Manual testing only, never write automated tests
- **Clear Constraints**: Provide explicit guidelines to prevent AI from going off-track

## Available Prompts

### 1. `create_task_batch`

**Purpose**: Break down a feature into well-defined tasks for a project

**When to use**: At the start of a work session to plan out tasks for 1-5 projects

**Parameters**:

- `projectName`: Name or identifier of the project (e.g., "E-commerce Dashboard", "Mobile App")
- `feature`: The feature or goal to accomplish (e.g., "Add user authentication", "Implement shopping cart")
- `constraints`: Your expert knowledge:
  - Existing code patterns to reuse
  - UX standards to follow
  - Workflow preferences
  - Anti-patterns to avoid (over-engineering, etc.)

**Example**:

```
Use prompt create_task_batch with:
- projectName: "E-commerce Dashboard"
- feature: "Add product search with filters"
- constraints: "Reuse the existing DataTable component from components/shared/DataTable.tsx.
  Follow the search pattern used in UserSearch.tsx. Keep UI minimal - just search box and
  filter dropdowns, no fancy animations. Use React Query for data fetching like we do everywhere
  else. Do NOT create a new search abstraction layer - keep it simple."
```

**Output**: Creates 3-5 tasks with:

- Clear objectives
- Files to modify
- Expected workflow
- What to avoid
- Task dependencies

---

### 2. `work_on_task`

**Purpose**: Execute a specific task with proper context and guardrails

**When to use**: After creating tasks, use this to have AI work on each task independently

**Parameters**:

- `taskId`: The task ID to work on
- `taskTitle`: Task title (from the task)
- `taskDescription`: Full task description with constraints
- `projectPath`: Absolute path to project root

**Example**:

```
Use prompt work_on_task with:
- taskId: 42
- taskTitle: "Implement product search API endpoint"
- taskDescription: "Create a search endpoint in /api/products/search. Reuse the existing
  database query builder from lib/db/queryBuilder.ts. Return paginated results. No fancy
  caching yet - keep it simple."
- projectPath: "C:/Projects/ecommerce-dashboard"
```

**What it does**:

1. Searches codebase for existing patterns
2. Uses Context7 MCP for library docs
3. Implements following KISS principle
4. Updates task with modified files
5. Marks task complete
6. Provides manual testing guide

---

### 3. `review_completed_tasks`

**Purpose**: Get a summary of all completed work across projects

**When to use**: After your 20-minute AI work session, before manual testing

**Parameters**: None

**Example**:

```
Use prompt review_completed_tasks
```

**Output**: A report with:

- Completed tasks grouped by project
- Key changes made
- Files modified
- Manual testing recommendations
- Any issues or blockers

---

### 4. `report_issue`

**Purpose**: Document issues found during manual testing

**When to use**: When you find bugs or problems while testing

**Parameters**:

- `taskId`: Which task has the issue
- `issueDescription`: What's wrong
- `expectedVsActual`: What should happen vs what actually happens

**Example**:

```
Use prompt report_issue with:
- taskId: 42
- issueDescription: "Search returns 500 error when query is empty"
- expectedVsActual: "Expected: Return empty array. Actual: Server crashes with
  'Cannot read property length of undefined'"
```

**What it does**:

1. Adds issue to the task
2. Updates task status to Blocked if needed
3. Provides reproduction steps
4. Suggests fix if obvious

---

### 5. `plan_next_iteration`

**Purpose**: Plan the next batch of tasks based on testing feedback

**When to use**: After manual testing, to plan what to work on next

**Parameters**:

- `feedback`: Overall feedback from testing
- `whatWorkedWell`: Patterns/approaches that succeeded
- `improvementAreas`: What needs to be fixed or improved

**Example**:

```
Use prompt plan_next_iteration with:
- feedback: "Search works great, but filters don't reset properly. Also the UX feels clunky."
- whatWorkedWell: "The DataTable reuse worked perfectly. Loading states are smooth."
- improvementAreas: "Need to fix filter reset bug. Improve UX by adding clear all button.
  Add keyboard shortcuts for power users."
```

**Output**: Next batch of prioritized tasks focusing on:

- Critical bug fixes
- Incomplete features
- Building on successful patterns
- UX refinements

---

## Typical Workflow

### Phase 1: Planning (5-10 minutes)

```
For each of your 4-5 projects:
1. Use create_task_batch to break down the feature
2. Review generated tasks, adjust if needed
3. Move to next project
```

### Phase 2: Execution (20 minutes, hands-off)

```
For each task across all projects:
1. Use work_on_task to let AI implement
2. AI searches codebase, reuses code, follows KISS
3. AI marks task complete when done
4. Repeat for all tasks (or as many as fit in 20 min)
```

### Phase 3: Review (5 minutes)

```
1. Use review_completed_tasks to see what was done
2. Read the summary
3. Prepare for manual testing
```

### Phase 4: Testing (10-15 minutes)

```
For each project:
1. Run the app
2. Test manually
3. If issues found, use report_issue for each
```

### Phase 5: Next Iteration (5 minutes)

```
1. Use plan_next_iteration with testing feedback
2. Review suggested tasks
3. Start Phase 2 again
```

---

## Key Benefits

### 1. **Context Preservation**

Your expert knowledge (constraints, patterns, preferences) is embedded in each prompt, so AI doesn't forget or go off-track.

### 2. **Code Reuse Enforcement**

The `work_on_task` prompt mandates searching codebase first, preventing duplicate code.

### 3. **KISS Principle**

All prompts emphasize simplicity over cleverness, preventing over-engineering.

### 4. **No Test Overhead**

Explicitly forbids AI from writing tests, saving time for your manual testing approach.

### 5. **Multi-Project Management**

Designed for working on 4-5 projects in batches, not one project at a time.

### 6. **Context7 Integration**

AI always fetches latest documentation, especially useful for fast-moving libraries like Next.js.

---

## Tips for Success

### Be Specific in Constraints

Bad: "Follow best practices"
Good: "Reuse UserForm.tsx pattern. Use React Hook Form like we do everywhere. No class components."

### Reference Existing Code

Bad: "Add authentication"
Good: "Add authentication using the same pattern as LoginForm.tsx. Use the useAuth hook from hooks/useAuth.ts"

### Specify What NOT to Do

Bad: "Add a modal"  
Good: "Add a simple modal using our Modal.tsx component. Do NOT create a modal management system or context provider - we don't need that complexity yet."

### Include Project Context

Bad: "Fix the bug"
Good: "Fix the bug in the product search. The issue is in api/products/search - it's the same query builder we use elsewhere, check lib/db/queryBuilder.ts for the pattern."

---

## Troubleshooting

### AI Still Over-Engineers

**Solution**: Be more explicit in constraints. Add "KEEP IT SIMPLE. No abstractions. No future-proofing. Just make it work."

### AI Doesn't Reuse Code

**Solution**: Explicitly mention the files to reuse in task description: "MUST reuse Button.tsx from components/ui/Button.tsx"

### AI Writes Tests Anyway

**Solution**: The prompts explicitly forbid this, but if it happens, add to constraints: "ABSOLUTELY NO TESTS. I will test manually."

### Tasks Too Large

**Solution**: In `create_task_batch`, add to constraints: "Break into very small tasks. Each task should touch 1-3 files max."

---

## Integration with Claude Desktop

Add this to your `claude_desktop_config.json`:

```json
{
  "mcpServers": {
    "taskmanagement": {
      "command": "taskmcp",
      "args": ["--db=path/to/.taskmanagement.db"]
    }
  }
}
```

Then in Claude:

```
@taskmanagement use prompt create_task_batch with projectName="MyProject" feature="Add login" constraints="..."
```

---

## Next Steps

1. **Start Small**: Try with 1-2 projects first
2. **Refine Constraints**: After first iteration, update constraints based on what AI got wrong
3. **Build a Library**: Keep a notes file of good constraint patterns that work for your style
4. **Iterate**: Each cycle improves as AI learns your patterns through the prompts

---

## Questions?

Remember: These prompts are templates. Customize the constraints to match YOUR coding style, YOUR project patterns, and YOUR preferences. The more specific you are, the better AI will follow your lead.
