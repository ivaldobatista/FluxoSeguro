using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PropostaService.Application.UseCases;
using PropostaService.Infrastructure;
using PropostaService.Infrastructure.Repositories;

namespace PropostaService.Tests.Application;

public class PropostaAppServiceTests
{
    private static PropostaUseCases BuildService()
    {
        var conn = new SqliteConnection("DataSource=:memory:");
        conn.Open();

        var options = new DbContextOptionsBuilder<PropostaDbContext>()
            .UseSqlite(conn)
            .Options;

        var ctx = new PropostaDbContext(options);
        ctx.Database.EnsureCreated();

        var repo = new PropostaRepository(ctx);
        return new PropostaUseCases(repo);
    }

    [Fact]
    public async Task Criar_Listar_AlterarStatus_FluxoHappyPath()
    {
        var svc = BuildService();
        var id = await svc.CriarPropostaAsync("Cliente A", 1000m);
        id.Should().NotBeEmpty();

        var list = await svc.ListarPropostasAsync();
        list.Should().ContainSingle(x => x.Id == id);

        await svc.AlterarStatusAsync(id, PropostaService.Domain.Entities.PropostaStatus.Aprovada);
        var after = await svc.ListarPropostasAsync();
        after.Single(x => x.Id == id).Status
             .Should().Be(PropostaService.Domain.Entities.PropostaStatus.Aprovada);
    }

    [Fact]
    public async Task Criar_DeveFalhar_SePayloadInvalido()
    {
        var svc = BuildService();
        await FluentActions.Invoking(() => svc.CriarPropostaAsync("", 1000m))
            .Should().ThrowAsync<ArgumentException>();
        await FluentActions.Invoking(() => svc.CriarPropostaAsync("Nome", -1))
            .Should().ThrowAsync<ArgumentException>();
    }
}
