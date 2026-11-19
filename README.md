# GjejVend

Parking marketplace for Pristina: owners list lots (location, price, hours, live availability); drivers find, compare and pre-book spots.

- **Backend:** ASP.NET Core 8 Web API, Clean Architecture (`backend/`)
- **Frontend:** React + TypeScript + Vite (`frontend/`)
- **Contract:** OpenAPI spec exposed by the API at `/swagger/v1/swagger.json`

## Quick start

```bash
cd backend && cp .env.example .env   # fill in the Supabase password
dotnet run --project src/GjejVend.Api
cd frontend && cp .env.example .env && npm install && npm run dev
```

## Database (Supabase)

The API uses the existing Supabase Postgres project through EF Core (`GjejVendDbContext` in `GjejVend.Infrastructure`).

- **Config:** the API reads `ConnectionStrings__Default`. Locally it is loaded from `backend/.env` (gitignored) at startup; real environment variables take precedence, so set it as an env var/secret in deployed environments. See `backend/.env.example`.
- **Host:** use the Supavisor **session pooler** (`aws-1-eu-west-1.pooler.supabase.com:5432`, user `postgres.<project-ref>`). The direct host `db.<project-ref>.supabase.co` is IPv6-only. Session mode suits this long-running API; Npgsql pools on the client side, capped by `Maximum Pool Size`.
- **Check:** `GET /health` returns `Healthy` when the database is reachable and `503 Unhealthy` otherwise.
- **Credentials stay server-side:** the frontend talks only to the API and never receives database credentials or Supabase keys.
- **Migrations:** none exist yet (no entities). When models are added, from `backend/`:
  `dotnet ef migrations add <Name> --project src/GjejVend.Infrastructure --startup-project src/GjejVend.Api`, then `dotnet ef database update ...` with the same flags.
- **Row Level Security:** the API connects as `postgres` and bypasses RLS, but Supabase also exposes the `public` schema through its Data API. Enable RLS on every new `public` table (`alter table ... enable row level security;`, in the migration) so it is not reachable with the anon key.
- **PostGIS:** available on the project but not installed; enable it in the first migration that needs spatial types.

`docker compose up -d db` still starts a local Postgres + PostGIS if you prefer to work offline; point `ConnectionStrings__Default` at it.
