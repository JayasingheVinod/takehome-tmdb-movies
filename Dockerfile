FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY TakeHome.TmdbMovies.sln ./
COPY src/TakeHome.TmdbMovies.Api/TakeHome.TmdbMovies.Api.csproj src/TakeHome.TmdbMovies.Api/
COPY src/TakeHome.TmdbMovies.Application/TakeHome.TmdbMovies.Application.csproj src/TakeHome.TmdbMovies.Application/
COPY src/TakeHome.TmdbMovies.Domain/TakeHome.TmdbMovies.Domain.csproj src/TakeHome.TmdbMovies.Domain/
COPY src/TakeHome.TmdbMovies.Infrastructure/TakeHome.TmdbMovies.Infrastructure.csproj src/TakeHome.TmdbMovies.Infrastructure/

RUN dotnet restore

COPY . .
RUN dotnet publish src/TakeHome.TmdbMovies.Api/TakeHome.TmdbMovies.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "TakeHome.TmdbMovies.Api.dll"]
