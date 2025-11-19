# MCP Task Manager - Quick Reference

## 🎯 Your Workflow at a Glance

```
┌─────────────────────────────────────────────────────────────┐
│  Phase 1: PLAN (5-10 min)                                   │
│  Use: create_task_batch for each project                    │
│  Goal: Create 3-5 tasks per project with clear constraints  │
└─────────────────────────────────────────────────────────────┘
                           ↓
┌─────────────────────────────────────────────────────────────┐
│  Phase 2: EXECUTE (20 min, hands-off)                       │
│  Use: work_on_task for each task                           │
│  AI: Searches, reuses code, follows KISS, marks complete    │
└─────────────────────────────────────────────────────────────┘
                           ↓
┌─────────────────────────────────────────────────────────────┐
│  Phase 3: REVIEW (5 min)                                     │
│  Use: review_completed_tasks                                 │
│  Goal: See what was done, prepare for testing              │
└─────────────────────────────────────────────────────────────┘
                           ↓
┌─────────────────────────────────────────────────────────────┐
│  Phase 4: TEST (10-15 min)                                   │
│  Manual testing, report issues                              │
│  Use: report_issue for each problem found                   │
└─────────────────────────────────────────────────────────────┘
                           ↓
┌─────────────────────────────────────────────────────────────┐
│  Phase 5: NEXT ITERATION (5 min)                            │
│  Use: plan_next_iteration                                    │
│  Goal: Plan next batch based on feedback                    │
└─────────────────────────────────────────────────────────────┘
```

## 🚀 Prompts Cheat Sheet

| Prompt                     | Use When                   | Key Parameters                                  |
| -------------------------- | -------------------------- | ----------------------------------------------- |
| **create_task_batch**      | Starting work on a feature | projectName, feature, constraints               |
| **work_on_task**           | AI should implement a task | taskId, taskTitle, taskDescription, projectPath |
| **review_completed_tasks** | Before testing             | (none)                                          |
| **report_issue**           | Found a bug during testing | taskId, issueDescription, expectedVsActual      |
| **plan_next_iteration**    | Planning next batch        | feedback, whatWorkedWell, improvementAreas      |

## 📋 Constraint Template

Copy/paste and customize for your style:

```
Reuse: [List existing files/components to reuse]
Pattern: [Which existing pattern to follow]
UX: [Specific UX requirements]
Libraries: [Use Context7 for: Next.js, React, Tailwind, etc.]
Keep It Simple: No abstractions, no over-engineering
NO TESTS: Manual testing only
Avoid: [Specific anti-patterns for your project]
```

## 🎨 Example Constraint (Next.js Project)

```
Reuse: components/ui/* for all UI elements, lib/db/prisma for database
Pattern: Follow the pattern in app/dashboard/page.tsx for layouts
UX: Use Tailwind, mobile-first, keep animations minimal
Libraries: Use Context7 for Next.js App Router, React Server Components, Prisma
Keep It Simple: No state management library, use React hooks only
NO TESTS: I will test manually
Avoid: Class components, client-side data fetching, complex state machines
```

## 🛠️ Available Tools

Use these tools directly when needed:

### Task Management

- `CreateTask` - Create a new task
- `UpdateTask` - Update task details
- `GetTask` - Get task details
- `ListTasks` - List all tasks
- `DeleteTask` - Delete a task

### Task Tracking

- `AddDependency` - Add task dependency
- `AddFileToTask` - Track modified files
- `AddTaskIssue` - Report an issue
- `AddSubTask` - Create a subtask

## 💡 Power Tips

### 1. Multi-Project Session

```
Morning session example:
- create_task_batch: E-commerce Dashboard (3 tasks)
- create_task_batch: Mobile App (4 tasks)
- create_task_batch: Admin Panel (3 tasks)
- create_task_batch: API Gateway (2 tasks)
Total: 12 tasks across 4 projects

Let AI work for 20 min → Review → Test → Next batch
```

### 2. Constraint Library

Keep a file with proven constraints:

```
my-constraints.md
├── nextjs-patterns.md
├── react-patterns.md
├── typescript-patterns.md
└── ux-standards.md
```

Then reference in constraints:

```
"Follow patterns from my-constraints/nextjs-patterns.md section 'Server Actions'"
```

### 3. Task Dependencies

Use dependencies for sequential work:

```
Task 1: Create database schema
Task 2: Create API endpoints (depends on Task 1)
Task 3: Create UI (depends on Task 2)
```

AI will respect these dependencies when using work_on_task.

### 4. Iterative Refinement

First iteration:

```
Constraints: "Keep it simple, reuse existing components"
```

After seeing AI's work:

```
Constraints: "Keep it simple, reuse Button from components/ui/Button.tsx,
use the same form pattern as LoginForm.tsx, NO inline styles - use Tailwind classes"
```

## 🔧 Troubleshooting Quick Fixes

| Problem               | Solution                                                             |
| --------------------- | -------------------------------------------------------------------- |
| AI over-engineers     | Add "KEEP IT SIMPLE. No abstractions." to constraints                |
| AI doesn't reuse code | Explicitly list files: "MUST reuse components/Card.tsx"              |
| AI writes tests       | Prompts forbid this, but add "NO TESTS" to constraints if needed     |
| Tasks too large       | Add to constraints: "Very small tasks. 1-3 files max each."          |
| AI ignores UX         | Be specific: "Exact copy of LoginForm.tsx layout. Use same spacing." |
| AI uses wrong library | "Use Context7 MCP to check latest [library name] docs"               |

## 📱 Quick Start Commands

### Build and Install

```powershell
cd "d:\tools\task manager"
dotnet build -c Release
dotnet pack TaskManagement.McpServer -c Release
dotnet tool install --global --add-source ./nupkg TaskManagement.McpServer
```

### Configure MCP

Add to `.vscode/mcp.json`:

```json
{
  "servers": {
    "TaskManagementMcp": {
      "type": "stdio",
      "command": "taskmcp",
      "args": ["--db=.taskmanagement.db"]
    }
  }
}
```

### First Use

```
1. Open Claude Code
2. Use prompt: create_task_batch
3. Fill in: projectName, feature, constraints
4. Review created tasks
5. Use prompt: work_on_task for each
6. Use prompt: review_completed_tasks
7. Test manually
8. Use prompt: report_issue if bugs found
9. Use prompt: plan_next_iteration
10. Repeat!
```

## 🎓 Remember

- **You're the architect**, AI is the implementer
- **Constraints = Your expertise codified**
- **KISS = Your productivity multiplier**
- **Manual testing = Your quality gate**
- **Batch work = Your scale advantage**

---

**Goal**: Transform from "babysitting AI task-by-task" to "orchestrating AI across 4-5 projects in batches"

You're now a **tech lead reviewing PRs**, not a developer writing every line. 🚀
