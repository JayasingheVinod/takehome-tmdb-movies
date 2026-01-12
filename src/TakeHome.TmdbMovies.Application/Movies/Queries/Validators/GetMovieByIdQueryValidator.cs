using FluentValidation;

namespace TakeHome.TmdbMovies.Application.Movies.Queries.Validators;

public sealed class GetMovieByIdQueryValidator : AbstractValidator<GetMovieByIdQuery>
{
    public GetMovieByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Id must be a positive integer.");
    }
}
