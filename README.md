# TakeHome TMDB Movies API

Clean Architecture ASP.NET Core Web API that caches TMDB movie data in SQL Server.

## Requirements Coverage (Quick)
- Public API: TMDB Movies (`/3/movie/popular`, `/3/movie/{id}`)
- Endpoints: list and get-by-id
- Caching: DB-first, fall back to TMDB and upsert
- DB access: raw SQL via `Microsoft.Data.SqlClient` (no ORM)
- Validation: FluentValidation + MediatR pipeline
- OpenAPI: Swagger UI enabled

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

## Configuration
Set these environment variables:
- `ExternalApis__Tmdb__ApiKey` (required)
- `ExternalApis__Tmdb__BaseUrl` (optional, default `https://api.themoviedb.org/3/`)
- `SA_PASSWORD` (required for docker-compose)
- `ConnectionStrings__SqlServer` (required for local run without docker-compose)

## Run With Docker (API + SQL Server)
```bash
export SA_PASSWORD='YourStrong@Passw0rd'
export ExternalApis__Tmdb__ApiKey='YOUR_TMDB_KEY'

docker compose up --build
```

API: `http://localhost:8080`  
Swagger UI: `http://localhost:8080/swagger`

## Run Locally (Mac + VS Code)
1. Start SQL Server in Docker:
```bash
docker run --name takehome-sql \
  -e 'ACCEPT_EULA=Y' \
  -e 'MSSQL_SA_PASSWORD=YourStrong@Passw0rd' \
  -p 1433:1433 \
  -d mcr.microsoft.com/mssql/server:2022-latest
```

2. Set environment variables and run:
```bash
export ConnectionStrings__SqlServer='Server=localhost,1433;Database=TakeHomeTmdbDb;User Id=sa;Password=YourStrong@Passw0rd;Encrypt=True;TrustServerCertificate=True;'
export ExternalApis__Tmdb__ApiKey='YOUR_TMDB_KEY'

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

## Database Schema
Schema is created on startup by `DatabaseInitializer`.  
Table: `dbo.Movies`

## Error Handling
- Validation errors return `400` with details.
- TMDB errors map to `502`/`503` with useful messages.
- Unhandled errors return `500`.
