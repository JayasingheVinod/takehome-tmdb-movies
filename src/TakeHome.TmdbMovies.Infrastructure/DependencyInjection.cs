using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TakeHome.TmdbMovies.Application.Abstractions;
using TakeHome.TmdbMovies.Infrastructure.Persistence;
using TakeHome.TmdbMovies.Infrastructure.Tmdb;

namespace TakeHome.TmdbMovies.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var cs = config.GetConnectionString("SqlServer")
                 ?? throw new InvalidOperationException("Missing ConnectionStrings:SqlServer");

        services.AddSingleton<IDbConnectionFactory>(_ => new SqlConnectionFactory(cs));
        services.AddSingleton<DatabaseInitializer>();
        services.AddScoped<IMovieRepository, MovieRepository>();

        services.Configure<TmdbOptions>(o =>
        {
            o.BaseUrl = config["ExternalApis:Tmdb:BaseUrl"] ?? "https://api.themoviedb.org/3/";
            o.ApiKey = config["ExternalApis:Tmdb:ApiKey"] ?? string.Empty;
        });

        services.AddHttpClient<ITmdbClient, TmdbClient>((sp, http) =>
        {
            var opt = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<TmdbOptions>>().Value;
            http.BaseAddress = new Uri(opt.BaseUrl);
            http.Timeout = TimeSpan.FromSeconds(15);
        });

        return services;
    }
}
