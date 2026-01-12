using TakeHome.TmdbMovies.Domain.Movies;

namespace TakeHome.TmdbMovies.Application.Abstractions;

public interface ITmdbClient
{
    Task<IReadOnlyList<MovieRecord>> FetchPopularAsync(int limit, CancellationToken ct);
    Task<MovieRecord?> FetchMovieByIdAsync(int id, CancellationToken ct);
}
