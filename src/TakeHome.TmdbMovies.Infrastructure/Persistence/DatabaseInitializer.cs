using Microsoft.Data.SqlClient;
using TakeHome.TmdbMovies.Application.Abstractions;

namespace TakeHome.TmdbMovies.Infrastructure.Persistence;

public sealed class DatabaseInitializer
{
    private readonly IDbConnectionFactory _db;

    public DatabaseInitializer(IDbConnectionFactory db) => _db = db;

    public async Task InitializeAsync(CancellationToken ct)
    {
        using var targetConn = (SqlConnection)_db.Create();
        var builder = new SqlConnectionStringBuilder(targetConn.ConnectionString);
        var targetDatabase = builder.InitialCatalog;

        if (!string.IsNullOrWhiteSpace(targetDatabase))
        {
            // The app cannot connect to the target DB until it exists.
            builder.InitialCatalog = "master";
            using var masterConn = new SqlConnection(builder.ConnectionString);
            await masterConn.OpenAsync(ct);

            const string createDatabaseSql = @"
IF DB_ID(@dbName) IS NULL
BEGIN
    DECLARE @sql NVARCHAR(4000) = N'CREATE DATABASE ' + QUOTENAME(@dbName);
    EXEC(@sql);
END
";

            using var createDb = new SqlCommand(createDatabaseSql, masterConn);
            createDb.Parameters.AddWithValue("@dbName", targetDatabase);
            await createDb.ExecuteNonQueryAsync(ct);
        }

        const string sql = @"
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Movies' AND xtype='U')
BEGIN
    CREATE TABLE dbo.Movies
    (
        Id INT NOT NULL PRIMARY KEY,
        Title NVARCHAR(300) NOT NULL,
        Overview NVARCHAR(MAX) NULL,
        ReleaseDate DATE NULL,
        Popularity FLOAT NULL,
        VoteAverage FLOAT NULL,
        VoteCount INT NULL,
        PosterPath NVARCHAR(500) NULL,
        BackdropPath NVARCHAR(500) NULL,
        OriginalLanguage NVARCHAR(20) NULL,
        GenresCsv NVARCHAR(500) NULL,
        CachedAtUtc DATETIME2 NOT NULL
    );

    CREATE INDEX IX_Movies_Title ON dbo.Movies(Title);
END
";

        await targetConn.OpenAsync(ct);

        using var cmd = new SqlCommand(sql, targetConn);
        await cmd.ExecuteNonQueryAsync(ct);
    }
}
