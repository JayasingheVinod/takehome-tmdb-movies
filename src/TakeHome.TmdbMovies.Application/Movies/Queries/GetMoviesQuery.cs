using MediatR;
using TakeHome.TmdbMovies.Application.Movies.Dtos;

namespace TakeHome.TmdbMovies.Application.Movies.Queries;

public sealed record GetMoviesQuery(int Limit) : IRequest<IReadOnlyList<MovieDto>>;
