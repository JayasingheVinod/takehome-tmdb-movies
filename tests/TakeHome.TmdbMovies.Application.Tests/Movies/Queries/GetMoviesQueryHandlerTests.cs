using Moq;
using TakeHome.TmdbMovies.Application.Abstractions;
using TakeHome.TmdbMovies.Application.Movies.Queries;
using TakeHome.TmdbMovies.Application.Movies.Queries.Handlers;
using TakeHome.TmdbMovies.Domain.Movies;

namespace TakeHome.TmdbMovies.Application.Tests.Movies.Queries;

public sealed class GetMoviesQueryHandlerTests
{
    [Fact]
    public async Task Handle_returns_cached_movies_when_cache_satisfies_limit()
    {
        var limit = 2;
        var cached = new List<MovieRecord>
        {
            CreateMovie(1),
            CreateMovie(2)
        };

        var repo = new Mock<IMovieRepository>();
        var tmdb = new Mock<ITmdbClient>();

        repo.Setup(r => r.GetListAsync(limit, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cached);

        var handler = new GetMoviesQueryHandler(repo.Object, tmdb.Object);

        var result = await handler.Handle(new GetMoviesQuery(limit), CancellationToken.None);

        Assert.Equal(limit, result.Count);
        Assert.Equal(new[] { 1, 2 }, result.Select(m => m.Id));

        tmdb.Verify(t => t.FetchPopularAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        repo.Verify(r => r.UpsertManyAsync(It.IsAny<IEnumerable<MovieRecord>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_fetches_from_tmdb_when_cache_is_insufficient()
    {
        var limit = 2;
        var cached = new List<MovieRecord> { CreateMovie(1) };
        var fetched = new List<MovieRecord>
        {
            CreateMovie(2),
            CreateMovie(3)
        };
        var nowCached = new List<MovieRecord>
        {
            CreateMovie(1),
            CreateMovie(2)
        };

        var repo = new Mock<IMovieRepository>();
        var tmdb = new Mock<ITmdbClient>();

        repo.SetupSequence(r => r.GetListAsync(limit, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cached)
            .ReturnsAsync(nowCached);

        tmdb.Setup(t => t.FetchPopularAsync(limit, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fetched);

        var handler = new GetMoviesQueryHandler(repo.Object, tmdb.Object);

        var result = await handler.Handle(new GetMoviesQuery(limit), CancellationToken.None);

        Assert.Equal(nowCached.Select(m => m.Id), result.Select(m => m.Id));

        tmdb.Verify(t => t.FetchPopularAsync(limit, It.IsAny<CancellationToken>()), Times.Once);
        repo.Verify(r => r.UpsertManyAsync(
            It.Is<IEnumerable<MovieRecord>>(records => records.Select(r => r.Id).SequenceEqual(fetched.Select(r => r.Id))),
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
