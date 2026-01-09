using TakeHome.TmdbMovies.Application.Movies.Queries;
using TakeHome.TmdbMovies.Application.Movies.Queries.Validators;

namespace TakeHome.TmdbMovies.Application.Tests.Movies.Queries;

public sealed class GetMoviesQueryValidatorTests
{
    private readonly GetMoviesQueryValidator _validator = new();

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    public void Rejects_out_of_range_limits(int limit)
    {
        var result = _validator.Validate(new GetMoviesQuery(limit));

        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(20)]
    [InlineData(100)]
    public void Accepts_valid_limits(int limit)
    {
        var result = _validator.Validate(new GetMoviesQuery(limit));

        Assert.True(result.IsValid);
    }
}
