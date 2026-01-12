using FluentValidation;

namespace TakeHome.TmdbMovies.Application.Movies.Queries.Validators;

public sealed class GetMoviesQueryValidator : AbstractValidator<GetMoviesQuery>
{
    public GetMoviesQueryValidator()
    {
        RuleFor(x => x.Limit)
            .InclusiveBetween(1, 100)
            .WithMessage("Limit must be between 1 and 100.");
    }
}
