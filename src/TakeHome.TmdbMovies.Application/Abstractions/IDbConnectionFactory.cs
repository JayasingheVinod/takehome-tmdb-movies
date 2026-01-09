using System.Data;

namespace TakeHome.TmdbMovies.Application.Abstractions;

public interface IDbConnectionFactory
{
    IDbConnection Create();
}
