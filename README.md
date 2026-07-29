# ReachingOutDB

## Overview

ReachingOutDB is an internal Blazor Server app for running a mailing/print shop's day-to-day operations. It replaces spreadsheets and manual tracking with a single database-backed system covering:

- **Customers** — customer records, mailing notes, and a change-history log.
- **Orders** — order entry, bulk creation, job status, bindery and Duplo progress tracking, and importing Endicia print logs from CSV.
- **Packages & shipping** — domestic and international package tracking, shipping calculator, UPS cost input and dashboard.
- **Plates** — press plate creation and manual plating workflow.
- **Reminders** — configurable reminder rules that email users automatically (via SMTP) when orders need attention.
- **Users** — user management and per-user customer view access.

Built with .NET 8, Blazor Server, Entity Framework Core, and PostgreSQL. UI components come from Syncfusion Blazor and Microsoft FluentUI.

## Tech stack

- **.NET 8 / Blazor Server** (interactive server render mode)
- **Entity Framework Core 9** with **Npgsql** (PostgreSQL provider)
- **Syncfusion Blazor** + **Microsoft FluentUI** for UI components
- **MailKit** for sending reminder emails
- Runs as a Docker container alongside a PostgreSQL container (see `docker-compose.yml`)

## Setup

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (for the containerized database and/or running the whole app in Docker)
- Visual Studio 2022 (recommended) or VS Code with the C# extension

### Option A: Run locally for development

1. **Clone the repo.**
   ```
   git clone https://github.com/drdotnet22/ReachingOutDB
   ```

2. **Get a PostgreSQL database running.** The easiest way is Docker:
   ```
   docker compose up postgres -d
   ```
   This starts just the `postgres` service from `docker-compose.yml`. By default it listens on port `5532` (mapped from Postgres's internal `5432`).

2. **Set your connection string.** `appsettings.Development.json` holds the connection string used when running in the `Development` environment (which Visual Studio uses by default). Update it to match your local Postgres instance, e.g.:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Host=localhost;Port=5532;Database=myapp;Username=postgres;Password=your_password"
   }
   ```
   Don't commit real passwords here — for anything beyond a local throwaway database, consider using [.NET user secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets) instead.

3. **Apply database migrations.** The app applies pending EF Core migrations automatically on startup (see `context.Database.Migrate()` in `Program.cs`), so you normally don't need to run anything manually — just start the app and it will create/update the schema. If you ever want to apply migrations without starting the app (e.g. to inspect the SQL first), you can run:
   ```
   dotnet ef database update
   ```
   (Requires the EF Core CLI tool: `dotnet tool install --global dotnet-ef` if you don't already have it.)

4. **Run the app.**
   - In Visual Studio: open `ReachingOutDB.sln` and press **F5** (or Ctrl+F5 to run without debugging).
   - From the command line:
     ```
     dotnet run
     ```
   The app will be available at the URL shown in the console output (typically `https://localhost:5001` or similar).

### Option B: Run the full stack with Docker

This runs both the app and its database as containers, close to how it runs in production.

1. Review `docker-compose.yml` and change the placeholder Postgres password (`your_secure_password`) to something secure.
2. From the project root:
   ```
   docker compose up --build -d
   ```
3. The app will be available at `http://localhost:10575` (per the port mapping in `docker-compose.yml`).

On startup, the app automatically applies any pending EF Core migrations against the `postgres` container's database — no manual migration step needed.

## Updates

To update a running deployment after pulling in code changes:

```
git pull
docker compose up --build -d
```

This rebuilds the `blazorapp` image with the latest code and restarts the container. The `postgres` container and its data volume (`postgres_data`) are left untouched. Any new EF Core migrations included in the update are applied automatically the next time the app starts — you don't need to run a separate migration command.

If you added a new EF Core migration during development (via `dotnet ef migrations add <Name>` locally), make sure to commit the generated files in the `Migrations/` folder — that's what lets the deployed app pick up and apply the schema change automatically.
