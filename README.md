# AI Project Intake & Estimation Assistant

Backend-first thesis demo built with ASP.NET Core 8, Clean Architecture, EF Core/SQLite, Identity, JWT, AutoMapper, and a failure-safe AI analyzer.

## Backend structure

- `src/Domain` — entities and enums
- `src/Application` — DTOs, interfaces, mapping, and business services
- `src/Infrastructure` — OpenAI integration, JWT creation, and development mail sender
- `src/Data` — EF Core context and repositories
- `src/API` — controllers, authentication/authorization, dependency injection, Swagger, and demo seeding

The separately scaffolded `frontend` folder is intentionally deferred; backend work is the current priority.

## Prerequisites

- .NET 8 SDK

## Run the API

```bash
dotnet restore AiProjectIntake.sln
dotnet run --project src/API/API.csproj
```

Open Swagger at `http://localhost:5080/swagger`. The health check is `GET http://localhost:5080/health`.

On first startup, SQLite is created automatically and roles, users, and three employees are seeded. This `EnsureCreated` approach keeps classroom demos reliable without a separate migration step.

Demo accounts:

| Role | Email | Password |
|---|---|---|
| Admin | `admin@demo.local` | `Admin123!` |
| Client | `client@demo.local` | `Client123!` |

Use `POST /api/auth/login`, copy the returned token, then select **Authorize** in Swagger and paste the token.

## Main endpoints

| Method | Endpoint | Access |
|---|---|---|
| POST | `/api/auth/register` | Public |
| POST | `/api/auth/login` | Public |
| GET/POST/PUT/DELETE | `/api/employees` | Admin |
| GET | `/api/project-requests` | Admin: all; Client: own |
| POST | `/api/project-requests` | Client |
| POST | `/api/project-requests/{id}/analyze` | Admin |
| POST | `/api/project-requests/{id}/send-reply` | Admin |
| GET | `/api/dashboard` | Admin |

## AI behavior

No API key is required for the demo. If `OpenAI:ApiKey`, `OPENAI_API_KEY`, and `AI_API_KEY` are all missing, the external call fails, times out, or returns incomplete JSON, the application returns a deterministic local analysis instead of failing. External calls have a 12-second timeout so a live demonstration remains responsive.

To enable the external integration without committing a key:

```bash
export OPENAI_API_KEY="your-key"
dotnet run --project src/API/API.csproj
```

`AI_API_KEY` is accepted as an alternative environment-variable name.

The model can be changed through `OpenAI:Model`. Review generated estimates before sending them to a client.

## Mail behavior

“Send reply” is simulated. The recipient, subject, and generated body are written to the API log, and the analysis receives a `ReplySentAt` timestamp.

## EF Core migrations

The initial demo uses `EnsureCreated` for zero-friction startup. When the entity model stabilizes, switch `EnsureCreatedAsync()` in `Program.cs` to `MigrateAsync()` and create the first migration:

```bash
dotnet tool install --global dotnet-ef
dotnet ef migrations add InitialCreate --project src/Data --startup-project src/API --output-dir Persistence/Migrations
dotnet ef database update --project src/Data --startup-project src/API
```

Do not mix an existing `EnsureCreated` database with migrations; delete the local `project-intake.db` once when making that switch.

## Demo flow

1. Log in as the seeded client and submit a project request.
2. Log in as admin and list requests.
3. Open a request and call **Analyze**.
4. Review the generated estimate and recommended seeded team.
5. Call **Send reply** to simulate mail delivery.
6. Check the dashboard counters.

## Configuration note

The JWT key in `appsettings.json` is a clearly marked local-demo value. Replace it with a secret supplied through environment/user-secrets before deploying anywhere outside a classroom demo.
