using MediatR;
using Microsoft.AspNetCore.Mvc;
using TakeHome.TmdbMovies.Application.Movies.Dtos;
using TakeHome.TmdbMovies.Application.Movies.Queries;

namespace TakeHome.TmdbMovies.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class MoviesController : ControllerBase
{
    private readonly IMediator _mediator;

    public MoviesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<MovieDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<MovieDto>>> Get([FromQuery] int limit = 20, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetMoviesQuery(limit), ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(MovieDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MovieDto>> GetById([FromRoute] int id, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetMovieByIdQuery(id), ct);
        return Ok(result);
    }
}
