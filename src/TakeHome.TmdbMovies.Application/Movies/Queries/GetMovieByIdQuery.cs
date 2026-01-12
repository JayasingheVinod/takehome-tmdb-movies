using MediatR;
using TakeHome.TmdbMovies.Application.Movies.Dtos;

namespace TakeHome.TmdbMovies.Application.Movies.Queries;

public sealed record GetMovieByIdQuery(int Id) : IRequest<MovieDto>;
