using GjejVend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Testcontainers.PostgreSql;

namespace GjejVend.Infrastructure.IntegrationTests;

public sealed class DatabaseFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder().WithImage("postgis/postgis:16-3.4").Build();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        await using var context = CreateContext();
        await context.Database.MigrateAsync();
    }

    public GjejVendDbContext CreateContext(ICurrentUser? currentUser = null)
    {
        var options = new DbContextOptionsBuilder<GjejVendDbContext>()
            .UseNpgsql(_container.GetConnectionString(), npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "private"));
        if (currentUser is not null)
        {
            options.AddInterceptors(new ActorConnectionInterceptor(currentUser));
        }

        return new GjejVendDbContext(options.Options);
    }

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();

    public async Task<NpgsqlConnection> OpenAsync()
    {
        var connection = new NpgsqlConnection(_container.GetConnectionString());
        await connection.OpenAsync();
        return connection;
    }
}

[CollectionDefinition(Name)]
public sealed class DatabaseGroup : ICollectionFixture<DatabaseFixture>
{
    public const string Name = "database";
}
