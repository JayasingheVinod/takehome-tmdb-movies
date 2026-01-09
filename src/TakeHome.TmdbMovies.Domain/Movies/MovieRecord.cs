namespace TakeHome.TmdbMovies.Domain.Movies;

public sealed class MovieRecord
{
    public int Id { get; init; }

    public string Title { get; init; } = string.Empty;
    public string? Overview { get; init; }
    public DateTime? ReleaseDate { get; init; }

    public double? Popularity { get; init; }
    public double? VoteAverage { get; init; }
    public int? VoteCount { get; init; }

    public string? PosterPath { get; init; }
    public string? BackdropPath { get; init; }
    public string? OriginalLanguage { get; init; }
    public string? GenresCsv { get; init; }

    public DateTime CachedAtUtc { get; init; }
}
