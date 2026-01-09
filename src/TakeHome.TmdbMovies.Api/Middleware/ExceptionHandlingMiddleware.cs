using System.Net;
using FluentValidation;
using TakeHome.TmdbMovies.Application.Common.Errors;

namespace TakeHome.TmdbMovies.Api.Middleware;

public sealed class ExceptionHandlingMiddleware : IMiddleware
{
    private readonly ILogger<ExceptionHandlingMiddleware> _log;

    public ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> log) => _log = log;

    public async Task InvokeAsync(HttpContext ctx, RequestDelegate next)
    {
        try
        {
            await next(ctx);
        }
        catch (ValidationException ex)
        {
            ctx.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            await ctx.Response.WriteAsJsonAsync(new
            {
                title = "Validation failed",
                status = 400,
                errors = ex.Errors.Select(e => new { field = e.PropertyName, message = e.ErrorMessage })
            });
        }
        catch (NotFoundException ex)
        {
            ctx.Response.StatusCode = (int)HttpStatusCode.NotFound;
            await ctx.Response.WriteAsJsonAsync(new { title = "Not found", status = 404, detail = ex.Message });
        }
        catch (ExternalServiceException ex)
        {
            // Map external service errors to something meaningful
            var status = ex.StatusCode switch
            {
                401 => 502,
                429 => 503,
                _ => 502
            };

            ctx.Response.StatusCode = status;
            await ctx.Response.WriteAsJsonAsync(new
            {
                title = "External service error",
                status,
                detail = ex.Message,
                externalStatus = ex.StatusCode
            });
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Unhandled exception");
            ctx.Response.StatusCode = 500;
            await ctx.Response.WriteAsJsonAsync(new { title = "Server error", status = 500 });
        }
    }
}
