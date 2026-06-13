# To-Do Task Manager

This repository contains a simple to-do task management application consisting of a .NET 10 minimal API backend and a Vue 3 frontend. The code present in this repository is what I was able to accomplish in roughly 2 - 2 1/2 business days (12-16 hours). The .NET API was primarily my own knowledge; the Vue client application was primarily AI tools (Claude & Copilot).

---

## Tech Stack
- Backend: .NET 10 minimal API (TaskManager.API)
  - Entity Framework Core (in-memory by default; SQLite option available)
- Frontend: Vue 3 + Vite (TaskManager.Vue)
- Tests: xUnit/.NET unit tests for backend, Vitest + Playwright for frontend

---

## Repo structure (top-level)
- TaskManager.API/         -- .NET minimal API project
- TaskManager.Vue/         -- Vue 3 frontend
- TaskManager.API.Testing.Unit/ -- backend unit tests

---

## Getting Started
- Open the TaskManager.API.sln in Visual Studio and build/run the TaskManager.API project or use the command line:
```
dotnet restore TaskManager.API\TaskManager.API.csproj
dotnet build TaskManager.API\TaskManager.API.csproj --no-restore
dotnet run --project TaskManager.API\TaskManager.API.csproj --no-build
```
- Open your terminal of choice to the TaskManager.Vue directory:
```
npm run build
npm run dev
```

## Vue Components
- AllLists.vue displays a collection of to-do list summaries provided by the user
- Tasks.vue displays a collection of tasks specific to each to-do list in the form of a summary and a completion checkbox

## API surface
Base path: /lists

- GET /lists
  - Returns summaries of lists. 200 OK or 500 on server error.
- GET /lists/{id}
  - Returns a list with its tasks. 200 OK, 404 if not found.
- GET /lists/{id}/tasks
  - Returns tasks for a list. 200 OK, 404 if not found.
- POST /lists
  - Body: ToDoListDto { summary, tasks? }
  - Validates summary (non-empty, <=50 chars). Returns 201 Created or 400 ValidationProblem.
- POST /lists/{id}/tasks
  - Body: ToDoTaskDto { summary, complete }
  - Validates summary (non-empty, <=50 chars). Returns 201 Created or 400 / 404.
- PUT /lists/{id}
  - Update list summary. Returns 200 or 404.
- PUT /lists/{id}/tasks/{taskId}
  - Update task (summary/complete). Returns 200 or 404.
- DELETE /lists/{id}
  - Delete list and its tasks. Returns 200 or 404.
- DELETE /lists/{id}/tasks/{taskId}
  - Delete a single task. Returns 200 or 404.

Navigate to https://localhost:7299/scalar for API docs and test harness

Notes: API uses endpoint filters for DTO validation and returns ProblemDetails for validation errors.

---

## API Architecture
The API approximates a clean architecture pattern in the form of "Domain" types defined in/used by the "Persistence" dependencies,
an Application "layer" (folder) containing application-specific validation (as opposed to domain invariants), and API-specific 
models being defined in the Models folder; the thought being the current implementation doesn't warrant separate projects in its
current form but if it were to grow the direction in which things should move should be evident.

## Client Architecture
Two components make up the bulk of the client functionality: AllLists.vue and Tasks.vue. API calls are handled by a standalone interface/dependency.

---

## API Error handling & non-happy paths
- Validation errors return 400 with a validation problem payload describing fields and errors.
- Not-found resources return 404.
- Unexpected server errors return 500. API endpoints catch common EF exceptions and return 500 for unexpected failures (TODO: add structured logging and correlation IDs).

---

## Assumptions & trade-offs
- In-memory DB chosen for simplicity during development and deterministic tests. Trade-off: data lost on restart. A production system should use a persisted store (SQLite for single-node MVP, Postgres for scale).
- Single-tenant app (no auth) for development. Adding per-user scoping is relatively straightforward (add UserId on lists and secure endpoints).
- Minimal surface area: no pagination on list endpoints (OK for small dataset). For large scale, add paging, filtering, and search indexes.
- API surface uses minimal APIs (MapGroup + handlers) for clarity; if app grows, move to controllers and services with DI for better testability/maintainability.

---

## Production MVP features to add (short list)
- Persistent DB (SQLite for small deployments, Postgres for scale) + migrations
- Authentication & authorization (JWT, OAuth) with per-user lists
- Input sanitization and rate limiting
- Request logging, structured logs, and monitoring
- Health checks, containerization (Dockerfile), orchestration (Aspire)
- Paging and filtering on list endpoints
- Expanded test coverage for automated CI runs (API and client apps)

---

## Tests and reliability
- API unit tests can be run with `dotnet test TaskManager.API.sln`
- Frontend unit tests (Vitest) and Playwright for smoke e2e tests can be run with `npm run test:unit` and `npm run test:e2e`

---
