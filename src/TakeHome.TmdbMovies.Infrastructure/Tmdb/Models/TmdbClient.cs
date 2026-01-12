using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using TakeHome.TmdbMovies.Application.Abstractions;
using TakeHome.TmdbMovies.Application.Common.Errors;
using TakeHome.TmdbMovies.Domain.Movies;
using TakeHome.TmdbMovies.Infrastructure.Tmdb.Models;

namespace TakeHome.TmdbMovies.Infrastructure.Tmdb;

public sealed class TmdbClient : ITmdbClient
{
    private readonly HttpClient _http;
    private readonly TmdbOptions _opt;

    public TmdbClient(HttpClient http, IOptions<TmdbOptions> opt)
    {
        _http = http;
        _opt = opt.Value;
    }

    public async Task<IReadOnlyList<MovieRecord>> FetchPopularAsync(int limit, CancellationToken ct)
    {
        EnsureApiKey();
        // TMDB popular is page-based, default page size is 20.
        // For take-home simplicity: fetch page 1 and take top 'limit' from results.
        var url = $"movie/popular?api_key={Uri.EscapeDataString(_opt.ApiKey)}";
        var res = await _http.GetAsync(url, ct);

        await EnsureSuccess(res, ct);

        var body = await res.Content.ReadFromJsonAsync<TmdbPopularResponse>(cancellationToken: ct)
                   ?? new TmdbPopularResponse();

        var now = DateTime.UtcNow;

        return body.Results
            .Take(limit)
            .Select(x => new MovieRecord
            {
                Id = x.Id,
                Title = x.Title,
                Overview = x.Overview,
                ReleaseDate = ParseDate(x.Release_Date),
                Popularity = x.Popularity,
                VoteAverage = x.Vote_Average,
                VoteCount = x.Vote_Count,
                PosterPath = x.Poster_Path,
                BackdropPath = x.Backdrop_Path,
                OriginalLanguage = x.Original_Language,
                CachedAtUtc = now
            })
            .ToList();
    }

    public async Task<MovieRecord?> FetchMovieByIdAsync(int id, CancellationToken ct)
    {
        EnsureApiKey();
        var url = $"movie/{id}?api_key={Uri.EscapeDataString(_opt.ApiKey)}";
        var res = await _http.GetAsync(url, ct);

        if (res.StatusCode == HttpStatusCode.NotFound)
            return null;

        await EnsureSuccess(res, ct);

        var body = await res.Content.ReadFromJsonAsync<TmdbMovieDetailsResponse>(cancellationToken: ct);
        if (body is null)
            throw new ExternalServiceException("TMDB returned an empty response.", (int)res.StatusCode);

        var genresCsv = body.Genres.Count == 0 ? null : string.Join(",", body.Genres.Select(g => g.Name));

        return new MovieRecord
        {
            Id = body.Id,
            Title = body.Title,
            Overview = body.Overview,
            ReleaseDate = ParseDate(body.Release_Date),
            Popularity = body.Popularity,
            VoteAverage = body.Vote_Average,
            VoteCount = body.Vote_Count,
            PosterPath = body.Poster_Path,
            BackdropPath = body.Backdrop_Path,
            OriginalLanguage = body.Original_Language,
            GenresCsv = genresCsv,
            CachedAtUtc = DateTime.UtcNow
        };
    }

    private static DateTime? ParseDate(string? yyyyMmDd)
        => DateTime.TryParse(yyyyMmDd, out var d) ? d.Date : null;

    private static async Task EnsureSuccess(HttpResponseMessage res, CancellationToken ct)
    {
        if (res.IsSuccessStatusCode)
            return;

        var status = (int)res.StatusCode;

        // Make error messages useful but not huge.
        var detail = await res.Content.ReadAsStringAsync(ct);

        if (res.StatusCode == HttpStatusCode.Unauthorized)
            throw new ExternalServiceException("TMDB unauthorized. Check API key.", status);

        if ((int)res.StatusCode == 429)
            throw new ExternalServiceException("TMDB rate limit reached (429). Try again later.", status);

        throw new ExternalServiceException($"TMDB error: HTTP {status}. {detail}", status);
    }

    private void EnsureApiKey()
    {
        if (!string.IsNullOrWhiteSpace(_opt.ApiKey))
            return;

        throw new ExternalServiceException(
            "TMDB API key is missing. Set ExternalApis__Tmdb__ApiKey.",
            401);
    }
}
