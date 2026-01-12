using System.Data;
using Microsoft.Data.SqlClient;
using TakeHome.TmdbMovies.Application.Abstractions;

namespace TakeHome.TmdbMovies.Infrastructure.Persistence;

public sealed class SqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public SqlConnectionFactory(string connectionString) => _connectionString = connectionString;

    public IDbConnection Create() => new SqlConnection(_connectionString);
}
