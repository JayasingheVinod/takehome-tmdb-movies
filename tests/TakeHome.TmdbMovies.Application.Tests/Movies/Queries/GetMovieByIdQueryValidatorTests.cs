using TakeHome.TmdbMovies.Application.Movies.Queries;
using TakeHome.TmdbMovies.Application.Movies.Queries.Validators;

namespace TakeHome.TmdbMovies.Application.Tests.Movies.Queries;

public sealed class GetMovieByIdQueryValidatorTests
{
    private readonly GetMovieByIdQueryValidator _validator = new();

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Rejects_non_positive_ids(int id)
    {
        var result = _validator.Validate(new GetMovieByIdQuery(id));

        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(42)]
    public void Accepts_positive_ids(int id)
    {
        var result = _validator.Validate(new GetMovieByIdQuery(id));

        Assert.True(result.IsValid);
    }
}
