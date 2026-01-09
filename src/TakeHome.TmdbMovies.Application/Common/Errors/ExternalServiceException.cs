namespace TakeHome.TmdbMovies.Application.Common.Errors;

public sealed class ExternalServiceException : Exception
{
    public int StatusCode { get; }

    public ExternalServiceException(string message, int statusCode) : base(message)
        => StatusCode = statusCode;
}
