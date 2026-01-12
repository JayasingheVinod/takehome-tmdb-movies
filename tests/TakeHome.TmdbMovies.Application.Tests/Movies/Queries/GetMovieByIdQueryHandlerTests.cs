using Moq;
using TakeHome.TmdbMovies.Application.Abstractions;
using TakeHome.TmdbMovies.Application.Common.Errors;
using TakeHome.TmdbMovies.Application.Movies.Queries;
using TakeHome.TmdbMovies.Application.Movies.Queries.Handlers;
using TakeHome.TmdbMovies.Domain.Movies;

namespace TakeHome.TmdbMovies.Application.Tests.Movies.Queries;

public sealed class GetMovieByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_returns_cached_movie_when_available()
    {
        var id = 10;
        var cached = CreateMovie(id);

        var repo = new Mock<IMovieRepository>();
        var tmdb = new Mock<ITmdbClient>();

        repo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cached);

        var handler = new GetMovieByIdQueryHandler(repo.Object, tmdb.Object);

        var result = await handler.Handle(new GetMovieByIdQuery(id), CancellationToken.None);

        Assert.Equal(id, result.Id);

        tmdb.Verify(t => t.FetchMovieByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        repo.Verify(r => r.UpsertAsync(It.IsAny<MovieRecord>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_throws_when_movie_not_found_in_tmdb()
    {
        var id = 999;

        var repo = new Mock<IMovieRepository>();
        var tmdb = new Mock<ITmdbClient>();

        repo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MovieRecord?)null);

        tmdb.Setup(t => t.FetchMovieByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MovieRecord?)null);

        var handler = new GetMovieByIdQueryHandler(repo.Object, tmdb.Object);

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(new GetMovieByIdQuery(id), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_fetches_from_tmdb_and_upserts_when_cache_misses()
    {
        var id = 42;
        var fetched = CreateMovie(id);

        var repo = new Mock<IMovieRepository>();
        var tmdb = new Mock<ITmdbClient>();

        repo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MovieRecord?)null);

        tmdb.Setup(t => t.FetchMovieByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fetched);

        var handler = new GetMovieByIdQueryHandler(repo.Object, tmdb.Object);

        var result = await handler.Handle(new GetMovieByIdQuery(id), CancellationToken.None);

        Assert.Equal(id, result.Id);

        repo.Verify(r => r.UpsertAsync(
            It.Is<MovieRecord>(record => record.Id == fetched.Id),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    private static MovieRecord CreateMovie(int id) => new()
    {
        Id = id,
        Title = $"Movie {id}",
        Overview = "Overview",
        ReleaseDate = new DateTime(2020, 1, 1),
        Popularity = 1.0,
        VoteAverage = 7.5,
        VoteCount = 100,
        PosterPath = "/poster.jpg",
        BackdropPath = "/backdrop.jpg",
        OriginalLanguage = "en",
        GenresCsv = "Action",
        CachedAtUtc = new DateTime(2020, 1, 2, 0, 0, 0, DateTimeKind.Utc)
    };
}
