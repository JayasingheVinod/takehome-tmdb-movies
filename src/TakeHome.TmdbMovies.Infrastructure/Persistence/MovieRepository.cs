using Microsoft.Data.SqlClient;
using TakeHome.TmdbMovies.Application.Abstractions;
using TakeHome.TmdbMovies.Domain.Movies;

namespace TakeHome.TmdbMovies.Infrastructure.Persistence;

public sealed class MovieRepository : IMovieRepository
{
    private readonly IDbConnectionFactory _db;

    public MovieRepository(IDbConnectionFactory db) => _db = db;

    public async Task<IReadOnlyList<MovieRecord>> GetListAsync(int limit, CancellationToken ct)
    {
        const string sql = @"
SELECT TOP (@Limit)
    Id, Title, Overview, ReleaseDate, Popularity, VoteAverage, VoteCount,
    PosterPath, BackdropPath, OriginalLanguage, GenresCsv, CachedAtUtc
FROM dbo.Movies
ORDER BY Popularity DESC, Id ASC;
";

        using var conn = (SqlConnection)_db.Create();
        await conn.OpenAsync(ct);

        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Limit", limit);

        using var reader = await cmd.ExecuteReaderAsync(ct);
        var list = new List<MovieRecord>();

        while (await reader.ReadAsync(ct))
        {
            list.Add(Map(reader));
        }

        return list;
    }

    public async Task<MovieRecord?> GetByIdAsync(int id, CancellationToken ct)
    {
        const string sql = @"
SELECT
    Id, Title, Overview, ReleaseDate, Popularity, VoteAverage, VoteCount,
    PosterPath, BackdropPath, OriginalLanguage, GenresCsv, CachedAtUtc
FROM dbo.Movies
WHERE Id = @Id;
";

        using var conn = (SqlConnection)_db.Create();
        await conn.OpenAsync(ct);

        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Id", id);

        using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return null;

        return Map(reader);
    }

    public async Task UpsertManyAsync(IEnumerable<MovieRecord> records, CancellationToken ct)
    {
        foreach (var r in records)
            await UpsertAsync(r, ct);
    }

    public async Task UpsertAsync(MovieRecord r, CancellationToken ct)
    {
        const string sql = @"
MERGE dbo.Movies AS target
USING (SELECT
    @Id AS Id,
    @Title AS Title,
    @Overview AS Overview,
    @ReleaseDate AS ReleaseDate,
    @Popularity AS Popularity,
    @VoteAverage AS VoteAverage,
    @VoteCount AS VoteCount,
    @PosterPath AS PosterPath,
    @BackdropPath AS BackdropPath,
    @OriginalLanguage AS OriginalLanguage,
    @GenresCsv AS GenresCsv,
    @CachedAtUtc AS CachedAtUtc
) AS source
ON target.Id = source.Id
WHEN MATCHED THEN
    UPDATE SET
        Title = source.Title,
        Overview = source.Overview,
        ReleaseDate = source.ReleaseDate,
        Popularity = source.Popularity,
        VoteAverage = source.VoteAverage,
        VoteCount = source.VoteCount,
        PosterPath = source.PosterPath,
        BackdropPath = source.BackdropPath,
        OriginalLanguage = source.OriginalLanguage,
        GenresCsv = source.GenresCsv,
        CachedAtUtc = source.CachedAtUtc
WHEN NOT MATCHED THEN
    INSERT (Id, Title, Overview, ReleaseDate, Popularity, VoteAverage, VoteCount,
            PosterPath, BackdropPath, OriginalLanguage, GenresCsv, CachedAtUtc)
    VALUES (source.Id, source.Title, source.Overview, source.ReleaseDate, source.Popularity, source.VoteAverage, source.VoteCount,
            source.PosterPath, source.BackdropPath, source.OriginalLanguage, source.GenresCsv, source.CachedAtUtc);
";

        using var conn = (SqlConnection)_db.Create();
        await conn.OpenAsync(ct);

        using var cmd = new SqlCommand(sql, conn);

        cmd.Parameters.AddWithValue("@Id", r.Id);
        cmd.Parameters.AddWithValue("@Title", r.Title);
        cmd.Parameters.AddWithValue("@Overview", (object?)r.Overview ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@ReleaseDate", (object?)r.ReleaseDate ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Popularity", (object?)r.Popularity ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@VoteAverage", (object?)r.VoteAverage ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@VoteCount", (object?)r.VoteCount ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@PosterPath", (object?)r.PosterPath ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@BackdropPath", (object?)r.BackdropPath ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@OriginalLanguage", (object?)r.OriginalLanguage ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@GenresCsv", (object?)r.GenresCsv ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@CachedAtUtc", r.CachedAtUtc);

        await cmd.ExecuteNonQueryAsync(ct);
    }

    private static MovieRecord Map(SqlDataReader reader)
    {
        DateTime? releaseDate = reader["ReleaseDate"] is DBNull ? null : (DateTime?)reader.GetDateTime(reader.GetOrdinal("ReleaseDate"));

        return new MovieRecord
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            Title = reader.GetString(reader.GetOrdinal("Title")),
            Overview = reader["Overview"] is DBNull ? null : reader.GetString(reader.GetOrdinal("Overview")),
            ReleaseDate = releaseDate,

            Popularity = reader["Popularity"] is DBNull ? null : (double?)reader.GetDouble(reader.GetOrdinal("Popularity")),
            VoteAverage = reader["VoteAverage"] is DBNull ? null : (double?)reader.GetDouble(reader.GetOrdinal("VoteAverage")),
            VoteCount = reader["VoteCount"] is DBNull ? null : (int?)reader.GetInt32(reader.GetOrdinal("VoteCount")),

            PosterPath = reader["PosterPath"] is DBNull ? null : reader.GetString(reader.GetOrdinal("PosterPath")),
            BackdropPath = reader["BackdropPath"] is DBNull ? null : reader.GetString(reader.GetOrdinal("BackdropPath")),
            OriginalLanguage = reader["OriginalLanguage"] is DBNull ? null : reader.GetString(reader.GetOrdinal("OriginalLanguage")),
            GenresCsv = reader["GenresCsv"] is DBNull ? null : reader.GetString(reader.GetOrdinal("GenresCsv")),

            CachedAtUtc = reader.GetDateTime(reader.GetOrdinal("CachedAtUtc"))
        };
    }
}
