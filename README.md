# AI Project Intake & Estimation Assistant

Backend-first thesis demo built with ASP.NET Core 8, Clean Architecture, EF Core/SQLite, Identity, JWT, AutoMapper, and a failure-safe AI analyzer.

## Backend structure

- `src/Domain` — entities and enums
- `src/Application` — DTOs, interfaces, mapping, and business services
- `src/Infrastructure` — local Ollama integration, JWT creation, and development mail sender
- `src/Data` — EF Core context and repositories
- `src/API` — controllers, authentication/authorization, dependency injection, Swagger, and demo seeding

The `frontend` folder contains the Next.js/TypeScript Material UI application for the complete admin and client demo flow.

## Prerequisites

- .NET 8 SDK
- Node.js 20 or newer

## Run the API

```bash
dotnet restore AiProjectIntake.sln
dotnet run --project src/API/API.csproj
```

Open Swagger at `http://localhost:5080/swagger`. The health check is `GET http://localhost:5080/health`.

## Run the frontend

```bash
cd frontend
cp .env.local.example .env.local
npm install
npm run dev
```

Open `http://localhost:3000`. `NEXT_PUBLIC_API_URL` defaults to `http://localhost:5080/api`.

Frontend routes:

- `/login` and `/register`
- `/admin/dashboard`
- `/admin/employees` and `/admin/employees/create`
- `/admin/project-requests` and `/admin/project-requests/[id]`
- `/client/submit-request` and `/client/my-requests`

Authentication is intentionally demo-oriented: the JWT response is stored in `localStorage`, and protected pages redirect users based on their `Admin` or `Client` role.

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

No API key is required. The analyzer uses Ollama locally at `http://127.0.0.1:11434/api/chat`. If Ollama is unavailable or returns incomplete JSON, the API reports the failure instead of presenting the deterministic safety fallback as genuine AI output.

Start Ollama and install the configured model:

```bash
ollama serve
ollama pull qwen3:4b
```

The endpoint and model can be changed through `Ollama:BaseUrl` and `Ollama:Model`. Review generated estimates before sending them to a client.

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
