# GYM Planner

A fitness-program generator. A deterministic C# engine computes the hard numbers
(BMI, BMR/TDEE, calorie + macro targets, training split/volume), and Claude turns
those numbers plus the customer's preferences into a human-friendly **weekly
program** returned as structured JSON.

> General fitness guidance only — **not medical advice**.

## Architecture

```
src/
  GYMPlanner.Domain/          pure calculation engine (no external deps)
  GYMPlanner.Application/      DTOs, IProgramGenerator, ProgramService
  GYMPlanner.Infrastructure/   EF Core + Identity, repository, Claude generator
  GYMPlanner.Api/             ASP.NET Core Web API (JWT auth, OpenAPI/Scalar)
web/                          React + TypeScript (Vite) frontend
tests/
  GYMPlanner.Domain.Tests/    xUnit tests for every calculator
```

- **.NET 10** (LTS) backend, **React 19 + TypeScript** frontend.
- **Auth:** ASP.NET Core Identity + JWT bearer tokens (shared by web and a future mobile app).
- **Database:** **PostgreSQL** (EF Core via Npgsql).

## Run with Docker (recommended)

The whole stack — PostgreSQL, the API, and the React app — runs with one command.
Only Docker is required (no .NET/Node install needed).

```bash
docker compose up --build
```

- web → **http://localhost:5173**
- api → **http://localhost:5030** (interactive reference at `/scalar/v1`)
- db → PostgreSQL on host port `5433` (data persists in the `pgdata` volume)

The API applies its EF migration automatically on startup. To enable AI program
generation, export `ANTHROPIC_API_KEY` before bringing the stack up:

```bash
export ANTHROPIC_API_KEY=sk-ant-...   # PowerShell: $env:ANTHROPIC_API_KEY="sk-ant-..."
docker compose up --build
```

Stop with `docker compose down` (add `-v` to also drop the database volume).

## Run locally without Docker

## Prerequisites

- .NET 10 SDK, Node 20+, the EF tools (`dotnet tool install --global dotnet-ef`),
  and a running **PostgreSQL** instance.

### Configure the database connection

The connection string lives under `ConnectionStrings:Default` in
[`appsettings.json`](src/GYMPlanner.Api/appsettings.json)
(`Host=localhost;Port=5432;Database=gymplanner;Username=postgres;Password=postgres`).
The app creates the `gymplanner` database and tables on first run.

Override the password (or the whole string) without committing a secret using
user-secrets:

```bash
dotnet user-secrets --project src/GYMPlanner.Api \
  set "ConnectionStrings:Default" "Host=localhost;Port=5432;Database=gymplanner;Username=postgres;Password=YOURPASSWORD"
```

(or set the `ConnectionStrings__Default` environment variable).

## Run the backend

```bash
dotnet run --project src/GYMPlanner.Api
```

- Listens on `http://localhost:5030` (see `src/GYMPlanner.Api/Properties/launchSettings.json`).
- Applies the EF migration automatically on startup (creates the `gymplanner` database + tables in PostgreSQL).
- Interactive API reference: `http://localhost:5030/scalar/v1`.

### Endpoints

| Method | Path | Auth | Purpose |
|--------|------|------|---------|
| POST | `/api/auth/register` | — | Create account, returns JWT |
| POST | `/api/auth/login` | — | Log in, returns JWT |
| POST | `/api/calculations` | — | Compute metrics from a profile (V1) |
| POST | `/api/programs` | JWT | Generate + save an AI weekly program (V2) |
| GET | `/api/programs` | JWT | List the signed-in customer's saved programs |

## Enable AI program generation (V2)

`POST /api/programs` calls the Claude API from the backend. Provide an API key
(the key never reaches the browser):

```bash
# PowerShell
$env:ANTHROPIC_API_KEY = "sk-ant-..."
dotnet run --project src/GYMPlanner.Api
```

Or set `Claude:ApiKey` in `appsettings.json`. The model defaults to
`claude-opus-4-8`; change `Claude:Model` to `claude-sonnet-4-6` /
`claude-haiku-4-5` to trade quality for cost. Without a key the endpoint returns
503 with a clear message — the rest of the app (the V1 calculator + accounts)
works regardless.

## Run the frontend

```bash
npm --prefix web install   # first time
npm --prefix web run dev   # http://localhost:5173
```

The API base URL is read from `web/.env` (`VITE_API_URL`, default
`http://localhost:5030`).

## Tests

```bash
dotnet test
```
