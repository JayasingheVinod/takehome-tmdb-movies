# TakeHome TMDB Movies API

Clean Architecture ASP.NET Core Web API that caches TMDB movie data in SQL Server.

## Interviewer Quick Start (Docker, recommended)
1. Add your TMDB API key to `.env` (compose loads it automatically):
```bash
SA_PASSWORD=Passw0rd123
ExternalApis__Tmdb__ApiKey=33082c5af5d846d731e43925a783ed87
```

2. Run the stack:
```bash
docker compose up --build
```

3. Open Swagger:
`http://localhost:8080/swagger`

4. Try it:
```bash
curl "http://localhost:8080/api/movies?limit=5"
curl "http://localhost:8080/api/movies/550"
```

## Requirements Coverage (Quick)
- Public API: TMDB Movies (`/3/movie/popular`, `/3/movie/{id}`)
- Endpoints: list and get-by-id
- Caching: DB-first, fall back to TMDB and upsert
- DB access: raw SQL via `Microsoft.Data.SqlClient` (no ORM)
- Validation: FluentValidation + MediatR pipeline
- OpenAPI: Swagger UI enabled

## Prerequisites
- Docker Desktop (for the quickest run)
- .NET SDK 8.0 (only for local non-docker run)
- TMDB API key from https://www.themoviedb.org/settings/api

## Configuration
Environment variables used by the API:

| Name | Required | Notes |
| --- | --- | --- |
| `ExternalApis__Tmdb__ApiKey` | yes | TMDB API key |
| `ExternalApis__Tmdb__BaseUrl` | no | Default: `https://api.themoviedb.org/3/` |
| `SA_PASSWORD` | yes (docker) | SQL Server SA password for compose |
| `ConnectionStrings__SqlServer` | yes (local) | SQL Server connection string |

## Run With Docker (API + SQL Server)
```bash
export SA_PASSWORD='Passw0rd123'
export ExternalApis__Tmdb__ApiKey='33082c5af5d846d731e43925a783ed87'

docker compose up --build
```

API: `http://localhost:8080`  
Swagger UI: `http://localhost:8080/swagger`

## Run Locally (API on host, SQL in Docker)
1. Start SQL Server in Docker:
```bash
docker run --name takehome-sql \
  -e 'ACCEPT_EULA=Y' \
  -e 'MSSQL_SA_PASSWORD=Passw0rd123' \
  -p 1433:1433 \
  -d mcr.microsoft.com/mssql/server:2022-latest
```

2. Set environment variables and run:
```bash
export ConnectionStrings__SqlServer='Server=localhost,1433;Database=TakeHomeTmdbDb;User Id=sa;Password=Passw0rd123;Encrypt=True;TrustServerCertificate=True;'
export ExternalApis__Tmdb__ApiKey='33082c5af5d846d731e43925a783ed87'

dotnet run --project src/TakeHome.TmdbMovies.Api
```

3. Open Swagger:
`http://localhost:5213/swagger` (or check console output for the port)

## Endpoints
- `GET /api/movies?limit=20`  
  Returns cached popular movies. If fewer than `limit` are cached, fetches from TMDB and upserts.
- `GET /api/movies/{id}`  
  Returns cached movie by ID. If missing, fetches from TMDB, saves, and returns.
- `GET /health`

## Architecture
Projects follow Clean Architecture:
- `TakeHome.TmdbMovies.Domain`: domain models
- `TakeHome.TmdbMovies.Application`: CQRS queries/handlers, validation, abstractions
- `TakeHome.TmdbMovies.Infrastructure`: SQL + TMDB integration
- `TakeHome.TmdbMovies.Api`: HTTP endpoints, middleware, Swagger

## Why These Libraries
- **MediatR**: CQRS-style request/handler separation and pipeline behaviors.
- **FluentValidation**: clear server-side validation for inputs.
- **Swashbuckle**: OpenAPI + Swagger UI.
- **Microsoft.Data.SqlClient**: direct SQL access without an ORM.
- **HttpClientFactory**: reliable, reusable HTTP clients.

## Database Schema
Schema is created on startup by `DatabaseInitializer`.  
Table: `dbo.Movies`

## Error Handling
- Validation errors return `400` with details.
- TMDB errors map to `502`/`503` with useful messages.
- Unhandled errors return `500`.

## Troubleshooting
- SQL Server can take 20-60 seconds to become ready; wait for compose healthchecks.
- If the API cannot connect, verify `SA_PASSWORD` and `ConnectionStrings__SqlServer` values.
- TMDB 401/403 usually means the API key is missing or invalid.
