using MediatR;
using TakeHome.TmdbMovies.Application.Abstractions;
using TakeHome.TmdbMovies.Application.Common.Errors;
using TakeHome.TmdbMovies.Application.Movies.Dtos;
using TakeHome.TmdbMovies.Domain.Movies;

namespace TakeHome.TmdbMovies.Application.Movies.Queries.Handlers;

public sealed class GetMovieByIdQueryHandler : IRequestHandler<GetMovieByIdQuery, MovieDto>
{
    private readonly IMovieRepository _repo;
    private readonly ITmdbClient _tmdb;

    public GetMovieByIdQueryHandler(IMovieRepository repo, ITmdbClient tmdb)
    {
        _repo = repo;
        _tmdb = tmdb;
    }

    public async Task<MovieDto> Handle(GetMovieByIdQuery request, CancellationToken ct)
    {
        var cached = await _repo.GetByIdAsync(request.Id, ct);
        if (cached is not null)
            return ToDto(cached);

        var fetched = await _tmdb.FetchMovieByIdAsync(request.Id, ct);
        if (fetched is null)
            throw new NotFoundException($"Movie with id {request.Id} was not found in TMDB.");

        await _repo.UpsertAsync(fetched, ct);
        return ToDto(fetched);
    }

    private static MovieDto ToDto(MovieRecord m) =>
        new(m.Id, m.Title, m.Overview, m.ReleaseDate, m.Popularity, m.VoteAverage, m.VoteCount,
            m.PosterPath, m.BackdropPath, m.OriginalLanguage, m.GenresCsv, m.CachedAtUtc);
}
