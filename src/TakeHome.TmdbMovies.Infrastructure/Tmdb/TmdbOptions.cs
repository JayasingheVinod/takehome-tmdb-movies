namespace TakeHome.TmdbMovies.Infrastructure.Tmdb;

public sealed class TmdbOptions
{
    public string BaseUrl { get; set; } = "https://api.themoviedb.org/3/";
    public string ApiKey { get; set; } = string.Empty;
}

