using TakeHome.TmdbMovies.Domain.Movies;

namespace TakeHome.TmdbMovies.Application.Abstractions;

public interface IMovieRepository
{
    Task<IReadOnlyList<MovieRecord>> GetListAsync(int limit, CancellationToken ct);
    Task<MovieRecord?> GetByIdAsync(int id, CancellationToken ct);

    Task UpsertManyAsync(IEnumerable<MovieRecord> records, CancellationToken ct);
    Task UpsertAsync(MovieRecord record, CancellationToken ct);
}
