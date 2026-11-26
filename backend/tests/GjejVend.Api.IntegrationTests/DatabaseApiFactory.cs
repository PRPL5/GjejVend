using System.Net.Http.Headers;
using GjejVend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using Testcontainers.PostgreSql;

namespace GjejVend.Api.IntegrationTests;

public sealed class DatabaseApiFactory : ApiFactory, IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder().WithImage("postgis/postgis:16-3.4").Build();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        await using var scope = Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<GjejVendDbContext>();
        if (context.Database.GetConnectionString() != _container.GetConnectionString())
        {
            throw new InvalidOperationException("The API is not connected to the test container.");
        }

        await context.Database.MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await _container.DisposeAsync();
    }

    public HttpClient ClientFor(Guid userId)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CreateToken(userId));
        return client;
    }

    public async Task<NpgsqlConnection> OpenAsync()
    {
        var connection = new NpgsqlConnection(_container.GetConnectionString());
        await connection.OpenAsync();
        return connection;
    }

    protected override void ConfigureServices(IServiceCollection services)
    {
        services.RemoveAll<DbContextOptions<GjejVendDbContext>>();
        services.RemoveAll<DbContextOptions>();
        services.AddDbContext<GjejVendDbContext>((provider, options) => options
            .UseNpgsql(_container.GetConnectionString(), npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "private"))
            .AddInterceptors(provider.GetRequiredService<ActorConnectionInterceptor>()));
    }
}
