# GjejVend

Parking marketplace for Pristina: owners list lots (location, price, hours, live availability); drivers find, compare and pre-book spots.

- **Backend:** ASP.NET Core 8 Web API, Clean Architecture (`backend/`)
- **Frontend:** React + TypeScript + Vite (`frontend/`)
- **Contract:** OpenAPI spec exposed by the API at `/swagger/v1/swagger.json`

## Quick start

```bash
docker compose up -d db          # Postgres + PostGIS
cd backend && dotnet run --project src/GjejVend.Api
cd frontend && cp .env.example .env && npm install && npm run dev
```

Or run everything: `docker compose up --build`.
