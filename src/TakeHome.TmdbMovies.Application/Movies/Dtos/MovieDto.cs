namespace TakeHome.TmdbMovies.Application.Movies.Dtos;

public sealed record MovieDto(
    int Id,
    string Title,
    string? Overview,
    DateTime? ReleaseDate,
    double? Popularity,
    double? VoteAverage,
    int? VoteCount,
    string? PosterPath,
    string? BackdropPath,
    string? OriginalLanguage,
    string? GenresCsv,
    DateTime CachedAtUtc
);
