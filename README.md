<div align="center">

<h1>🌿 TaskBoard</h1>

<h3>A full-stack task management app built with .NET 8</h3>

<p><em>Clean architecture · REST API · Blazor Server · Still in progress</em></p>

<br>

<p>
  <img src="https://img.shields.io/badge/.NET-8.0-1B5E20?style=for-the-badge&logo=dotnet&logoColor=white" alt=".NET 8">
  <img src="https://img.shields.io/badge/Blazor-Server-2E7D32?style=for-the-badge&logo=blazor&logoColor=white" alt="Blazor">
  <img src="https://img.shields.io/badge/EF%20Core-9.0-388E3C?style=for-the-badge&logo=nuget&logoColor=white" alt="EF Core">
  <img src="https://img.shields.io/badge/Tests-xUnit-43A047?style=for-the-badge&logo=testinglibrary&logoColor=white" alt="xUnit">
  <img src="https://img.shields.io/badge/Docs-Swagger-4CAF50?style=for-the-badge&logo=swagger&logoColor=white" alt="Swagger">
</p>

<br>

<p>
  <a href="#-getting-started">Getting Started</a> ·
  <a href="#-api-endpoints">API Endpoints</a> ·
  <a href="#-running-tests">Running Tests</a> ·
  <a href="#-architecture">Architecture</a>
</p>

</div>

<br>

---

<br>

## 🚀 Getting Started

### 1️⃣ Clone the repository

```bash
git clone https://github.com/Vaclavselir/Taskboard.git
cd Taskboard
```

<br>

### 2️⃣ Set up user secrets

> [!IMPORTANT]
> Sensitive values (JWT key, admin credentials, API key, database connection string) are **never stored in the repository**. You must configure them via `dotnet user-secrets` before running the app.

**API project:**

```bash
cd TaskBoard.Api

# JWT authentication key (must be at least 32 characters)
dotnet user-secrets set "Jwt:SecretKey" "your-secret-key-at-least-32-characters-long"

# API key for admin endpoints
dotnet user-secrets set "Security:ApiKey" "your-admin-api-key"

# Database connection string (only needed if using SQL storage)
dotnet user-secrets set "ConnectionStrings:dbTaskBoard" "Server=YOUR_SERVER_NAME\\SQLEXPRESS;Database=dbTaskBoard;Trusted_Connection=True;TrustServerCertificate=True"
```

Replace `YOUR_SERVER_NAME\\SQLEXPRESS` with your actual SQL Server instance name (e.g. `localhost\\SQLEXPRESS`, `.\\SQLEXPRESS`, or just `.` for the default instance).

**Admin account** — the app seeds a default admin user on startup. To configure the admin credentials:

```bash
dotnet user-secrets set "AdminAccount:Email" "admin@taskboard.com"
dotnet user-secrets set "AdminAccount:Password" "YourAdminPassword123!"
```

> [!NOTE]
> If admin secrets are missing, the app still starts normally — it just skips admin seeding and logs a warning. The connection string is only needed when using SQL storage (see step 3).

**UI project** (needs the same API key for the admin export page):

```bash
cd ../TaskBoard.UI
dotnet user-secrets set "Security:ApiKey" "your-admin-api-key"
```

<br>

### 3️⃣ Choose your storage

The app supports two storage backends. Open `TaskBoard.Api/appsettings.json` and set the provider:

```json
"Storage": {
  "Provider": "SQL",
  "Json": {
    "FilePath": "App_Data/tasks.json"
  }
}
```

| Value | Effect |
|:------|:-------|
| 🗄️ `"SQL"` | Uses EF Core with SQL Server — requires a running SQL Server instance and migrations (see below) |
| 📂 `"Json"` | Uses JSON file storage — **no database needed**, zero setup, great for quick testing |

> [!TIP]
> Both providers implement the same `ITaskRepository` interface, so the rest of the application works identically regardless of which one you choose.

<br>

<details>
<summary>🗄️ <strong>SQL Server setup</strong> (skip this section if using JSON)</summary>

<br>

> [!NOTE]
> The database connection string is configured via user-secrets (step 2). Make sure you've set `ConnectionStrings:dbTaskBoard` with your actual server name before proceeding.

Apply the database migrations:

```bash
cd TaskBoard.Api
dotnet ef database update
```

This creates the `dbTaskBoard` database with all required tables (`Tasks`, `Users`) and indexes.

</details>

<br>

### 4️⃣ Run the application

The solution consists of two projects that need to run simultaneously — the **API** (backend) and the **UI** (frontend). Start each in a separate terminal:

```bash
# Terminal 1 — start the API
cd TaskBoard.Api
dotnet run

# Terminal 2 — start the Blazor UI
cd TaskBoard.UI
dotnet run
```

Once both are running:

| Service | What you'll see |
|:--------|:----------------|
| 🌐 **API + Swagger** | Interactive API documentation at `https://localhost:{port}/swagger` |
| 🖥️ **Blazor UI** | The main application — unauthenticated users are redirected to the login page |

> [!TIP]
> You can register a new user account directly through the UI. The admin account (if configured in step 2) can be used to log in and access the admin export page.

<br>

---

<br>

## 🌲 Architecture

The solution follows a **layered clean architecture** with clear dependency flow — each layer only references the layer directly below it:

```
TaskBoard.sln
 ├─ TaskBoard.Domain          → Entities, enums, value objects, domain exceptions
 ├─ TaskBoard.Application     → Use cases, service interfaces (ITaskRepository, IAuth, ITime…)
 ├─ TaskBoard.Infrastructure  → EF Core + JSON persistence, JWT token service, password hashing
 ├─ TaskBoard.Api             → ASP.NET Core Web API (controllers, middleware, Swagger)
 ├─ TaskBoard.UI              → Blazor Server frontend (pages, auth state, API clients)
 ├─ TaskBoard.ConsoleApp      → Utility console runner
 └─ TaskBoard.Tests           → xUnit + Moq + FluentAssertions
```

**Why this structure?**

🔹 **Domain has zero infrastructure dependencies** — entities enforce their own rules (e.g. status can only move `Todo → Doing → Done`, never backwards). If you try an invalid transition, the domain throws a `ConflictException` before anything reaches the database.

🔹 **Application layer defines abstractions** (`ITaskRepository`, `ITime`, `IGeneratorId`) — Infrastructure provides the real implementations. This means business logic is fully testable with mocks, no database needed.

🔹 **Storage is swappable via one config value** — a single extension method (`AddTaskBoardStorage()`) reads `Storage:Provider` and registers either `EFRepository` or `JsonRepository` behind the same interface.

<br>

---

<br>

## 🌿 Features

| Requirement | Implementation |
|:---|:---|
| 🖥️ **UI** | Blazor Server — full CRUD, filtering by status/priority/tags, pagination, dark bioluminescent theme |
| 📋 **Data fields** | Title, Description, Priority (enum), Status (enum), DueDate, CreatedAt, UpdatedAt, Tags, IsOverdue (computed bool), LastCheckedAt, Email (on User), IsAdmin (bool on User) |
| 🗄️ **SQL storage** | Entity Framework Core with SQL Server and migrations |
| 📂 **JSON storage** | Custom `JsonRepository` with `SemaphoreSlim` for thread safety |
| ⚙️ **Storage switch** | One config value in `appsettings.json` swaps the entire storage layer |
| 🌐 **REST API** | Full CRUD on `/api/tasks` with pagination and query filters |
| 📖 **API docs** | Swagger UI with both JWT Bearer and API Key auth definitions |
| 🔐 **Security** | JWT Bearer tokens for user endpoints, API Key (`X-API-KEY` header) for admin |
| ⏱️ **Background service** | `TaskStatisticsService` — runs every 5 minutes, scans all tasks, logs overdue statistics, updates `LastCheckedAt` |
| ✅ **Unit tests** | xUnit + Moq + FluentAssertions — 5 test classes covering services and domain logic |

<br>

---

<br>

## 🔗 API Endpoints

All endpoints return JSON. Authentication is via JWT Bearer token (obtained from `/api/auth/login`).

<details>
<summary>🔓 <strong>Authentication</strong> — register & login (no auth required)</summary>

<br>

| Method | Route | Description |
|:-------|:------|:------------|
| `POST` | `/api/auth/register` | Register a new user — returns a JWT token |
| `POST` | `/api/auth/login` | Login with email + password — returns a JWT token |

**Example login request:**
```json
{
  "email": "user@example.com",
  "password": "YourPassword123"
}
```

</details>

<details>
<summary>📋 <strong>Tasks</strong> — CRUD operations (JWT Bearer token required)</summary>

<br>

| Method | Route | Description |
|:-------|:------|:------------|
| `GET` | `/api/tasks` | List your tasks — supports pagination and filtering by `status`, `priority`, `tags` |
| `GET` | `/api/tasks/{id}` | Get a single task — response includes an `ETag` header for concurrency |
| `POST` | `/api/tasks` | Create a new task |
| `PATCH` | `/api/tasks/{id}` | Update a task — send only the fields you want to change. Supports optimistic concurrency via `If-Match` header |
| `DELETE` | `/api/tasks/{id}` | Delete a task |

**Pagination example:** `GET /api/tasks?pageNumber=1&pageSize=10&status=doing&priority=high`

</details>

<details>
<summary>🛡️ <strong>Admin</strong> — export all tasks (API Key required)</summary>

<br>

| Method | Route | Description |
|:-------|:------|:------------|
| `GET` | `/api/admin/tasks` | Export all tasks across all users |

Pass the API key in the `X-API-KEY` header. This endpoint is protected by `KeyMiddleware` and does not require a JWT token.

</details>

<br>

---

<br>

## ✅ Running Tests

```bash
dotnet test
```

Tests use **Moq** for mocking abstractions (`ITaskRepository`, `ITime`, `IGeneratorId`) and **FluentAssertions** for readable assertions.

<details>
<summary>📊 <strong>What's covered</strong></summary>

<br>

| Test class | What it tests |
|:-----------|:--------------|
| `CreateTaskTest` | Input validation, ID generation, correct `CreatedAt`, priority and tags assignment |
| `UpdateTaskTests` | Field changes, "no change" detection (skips `Save()`), status transitions, `ConflictException` on invalid transitions, multi-field updates, `TaskUpdated` event |
| `DeleteTaskTests` | Owner validation, `Remove` → `Save` call order, `TaskDeleted` event firing |
| `ListTaskTests` | Filtering by status, priority, tags, empty results, combined filters |
| `TaskItemTests` | Constructor validation (title length, empty ID, date constraints), status state machine, `IsOverdue` computed property, update methods |

</details>

<br>

---

<br>

## 🧰 Tech Stack

| | Technology |
|:--|:-----------|
| ⚡ Runtime | .NET 8 / C# 12 |
| 🌐 API | ASP.NET Core Web API, Swagger (Swashbuckle) |
| 🖥️ UI | Blazor Server, Bootstrap 5.1, custom dark CSS theme |
| 🗄️ ORM | Entity Framework Core 9 (SQL Server) |
| 🔐 Auth | JWT Bearer + API Key middleware |
| ✅ Testing | xUnit, Moq, FluentAssertions |

<br>

---

<br>

## 💡 Design Highlights

<details>
<summary>🔄 <strong>Domain-driven status transitions</strong></summary>

<br>

`TaskItem.UpdateStatus()` enforces a strict state machine:

```
Todo  →  Doing  →  Done
```

Attempting to skip a step (e.g. `Todo → Done`) or go backwards (e.g. `Doing → Todo`) throws a `ConflictException`. This keeps business rules in the domain layer where they belong — no controller or service can bypass them.

</details>

<details>
<summary>⏰ <strong>Background service & overdue detection</strong></summary>

<br>

`IsOverdue` is a **computed property** on `TaskItem` — it's never stored in the database. A task is overdue when `DueDate < UtcNow` and `Status != Done`.

The `TaskStatisticsService` is a hosted background service that runs every 5 minutes. It scans all tasks, updates the `LastCheckedAt` timestamp on each one, and logs statistics (total, todo, doing, done, overdue counts) to the console.

</details>

<details>
<summary>🔀 <strong>Dual storage behind one interface</strong></summary>

<br>

Both `EFRepository` (SQL Server) and `JsonRepository` (file-based) implement `ITaskRepository` and `IUserRepository`. The `DependencyInjection.AddTaskBoardStorage()` extension method reads `Storage:Provider` from configuration and registers the appropriate implementation. The rest of the application — services, controllers, UI clients — has no idea which storage backend is active.

The JSON repository uses `SemaphoreSlim` for thread safety, since multiple requests can arrive concurrently in the API.

</details>

<details>
<summary>🛡️ <strong>Security details</strong></summary>

<br>

- **JWT Bearer authentication** — tokens include user ID and role claims. User endpoints require a valid token; the admin role-check endpoint uses `[Authorize(Roles = "Admin")]`.
- **API Key middleware** — the `/api/admin/*` routes are protected by `KeyMiddleware`, which uses `CryptographicOperations.FixedTimeEquals` for constant-time comparison (prevents timing attacks).
- **Admin seeder** — on startup, the app checks for `AdminAccount:Email` and `AdminAccount:Password` in configuration. If present and no admin exists yet, it creates one.
- **No secrets in the repo** — JWT keys, API keys, admin credentials, and the database connection string are all managed via `dotnet user-secrets`.

</details>

<details>
<summary>🔒 <strong>Optimistic concurrency</strong></summary>

<br>

Task updates use ETag-based concurrency control:

1. `GET /api/tasks/{id}` returns an `ETag` header containing the current `RowVersion`
2. `PATCH /api/tasks/{id}` accepts an `If-Match` header with the expected version
3. If another user modified the task in the meantime, the API returns `409 Conflict`

This prevents "last write wins" problems when multiple users edit the same task.

</details>

<details>
<summary>🔌 <strong>Blazor Server authentication</strong></summary>

<br>

Blazor Server runs over a persistent SignalR connection (called a "circuit"). The JWT token is stored in a circuit-scoped `TokenStore` service — one instance per user/tab. The UI project uses a custom `AuthenticationStateProvider` (`AuthStateService`) that parses the JWT token client-side to determine the user's identity, without calling the API on every page navigation.

</details>

<br>

---

<div align="center">

<br>

<em>Built as a .NET developer assignment · 2025 © Václav Šelíř</em>

<br>

</div>
