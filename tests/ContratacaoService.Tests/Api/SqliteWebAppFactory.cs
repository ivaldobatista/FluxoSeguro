using ContratacaoService.Application.Ports;
using ContratacaoService.Infrastructure;
using ContratacaoService.Tests.TestDoubles;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ContratacaoService.Tests.Api;

public class SqliteWebAppFactory : WebApplicationFactory<Program>
{
    private SqliteConnection? _connection;
    public FakePropostaGateway Gateway { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            var dbDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ContratacaoDbContext>));
            if (dbDescriptor != null) services.Remove(dbDescriptor);

            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            services.AddDbContext<ContratacaoDbContext>(opts => opts.UseSqlite(_connection));

            using var scope = services.BuildServiceProvider().CreateScope();
            var ctx = scope.ServiceProvider.GetRequiredService<ContratacaoDbContext>();

            ctx.Database.EnsureDeleted(); // isola cada execução
            ctx.Database.Migrate();       // usa o pipeline de migrations (igual produção)

            var gwDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IPropostaGateway));
            if (gwDescriptor != null) services.Remove(gwDescriptor);

            services.AddSingleton(Gateway);
            services.AddSingleton<IPropostaGateway>(sp => sp.GetRequiredService<FakePropostaGateway>());
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _connection?.Dispose();
    }
}
