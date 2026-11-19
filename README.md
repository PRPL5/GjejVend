# GjejVend

Parking marketplace for Pristina: owners list lots (location, price, hours, live availability); drivers find, compare and pre-book spots.

- **Backend:** ASP.NET Core 8 Web API, layered architecture (`backend/`)
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
- **Credentials stay server-side:** the frontend never receives database credentials or the service role key. Identity uses Supabase Auth (phone, Google, Apple); the API validates its JWT and enforces all authorization.
- **Schema:** 26 tables in `public`, one EF Core migration per module (`Foundation`, `Identity`, `Listings`, `Pricing`, `Bookings`, `Payments`, `Reviews`, `Engagement`, `Moderation`, `Search`, `Storage`), plus `Hardening`, in `GjejVend.Infrastructure/Persistence/Migrations`. Never edit an applied migration; add a new one. Raw SQL inside them adds PostGIS, exclusion constraints, triggers, functions, RLS and Storage buckets. The EF history table lives in `private.__EFMigrationsHistory`.
- **Migrations:** from `backend/`, run `dotnet tool restore` once, then `dotnet ef migrations add <Name> --project src/GjejVend.Infrastructure --startup-project src/GjejVend.Api --output-dir Persistence/Migrations` and `dotnet ef database update --project src/GjejVend.Infrastructure --startup-project src/GjejVend.Api`.
- **Row Level Security:** RLS is enabled on every `public` table with no policies, and `anon`/`authenticated` have no table or function privileges, so the Data API exposes nothing. Every new table must call `migrationBuilder.ProtectTables(...)`. The API connects as `postgres` and bypasses RLS.
- **Bookings:** create and extend bookings only through `create_booking` / `extend_booking`. They lock the location, check opening hours (in the location timezone), overrides and capacity, and price the booking. An exclusion constraint prevents overlapping bookings of the same space. `pg_cron` expires unpaid pending bookings every minute. Set `app.actor_id` per transaction so status history and audit logs record who acted.
- **Tests:** `dotnet test` runs the integration tests against a disposable `postgis/postgis:16-3.4` container (Docker required).

`docker compose up -d db` still starts a local Postgres + PostGIS if you prefer to work offline; point `ConnectionStrings__Default` at it.
