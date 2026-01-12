using MediatR;
using TakeHome.TmdbMovies.Application.Abstractions;
using TakeHome.TmdbMovies.Application.Movies.Dtos;
using TakeHome.TmdbMovies.Domain.Movies;

namespace TakeHome.TmdbMovies.Application.Movies.Queries.Handlers;

public sealed class GetMoviesQueryHandler : IRequestHandler<GetMoviesQuery, IReadOnlyList<MovieDto>>
{
    private readonly IMovieRepository _repo;
    private readonly ITmdbClient _tmdb;

    public GetMoviesQueryHandler(IMovieRepository repo, ITmdbClient tmdb)
    {
        _repo = repo;
        _tmdb = tmdb;
    }

    public async Task<IReadOnlyList<MovieDto>> Handle(GetMoviesQuery request, CancellationToken ct)
    {
        // Requirement: if data is already available in DB, return it (local cache).
        var cached = await _repo.GetListAsync(request.Limit, ct);
        if (cached.Count >= request.Limit)
            return cached.Select(ToDto).ToList();

        // Otherwise fetch from TMDB popular list, save, return.
        var fetched = await _tmdb.FetchPopularAsync(request.Limit, ct);
        await _repo.UpsertManyAsync(fetched, ct);

        var nowCached = await _repo.GetListAsync(request.Limit, ct);
        return nowCached.Select(ToDto).ToList();
    }

    private static MovieDto ToDto(MovieRecord m) =>
        new(m.Id, m.Title, m.Overview, m.ReleaseDate, m.Popularity, m.VoteAverage, m.VoteCount,
            m.PosterPath, m.BackdropPath, m.OriginalLanguage, m.GenresCsv, m.CachedAtUtc);
}
