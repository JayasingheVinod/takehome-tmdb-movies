namespace TakeHome.TmdbMovies.Infrastructure.Tmdb.Models;

public sealed class TmdbPopularResponse
{
    public int Page { get; set; }
    public List<TmdbMovieItem> Results { get; set; } = new();

    public sealed class TmdbMovieItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Overview { get; set; }
        public string? Release_Date { get; set; }
        public double? Popularity { get; set; }
        public double? Vote_Average { get; set; }
        public int? Vote_Count { get; set; }
        public string? Poster_Path { get; set; }
        public string? Backdrop_Path { get; set; }
        public string? Original_Language { get; set; }
    }
}
