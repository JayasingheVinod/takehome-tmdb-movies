namespace TakeHome.TmdbMovies.Application.Common.Errors;

public sealed class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}
