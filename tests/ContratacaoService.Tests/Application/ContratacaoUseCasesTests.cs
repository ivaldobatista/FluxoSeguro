using ContratacaoService.Application.Interfaces;
using ContratacaoService.Application.Ports;
using ContratacaoService.Application.UseCases;
using ContratacaoService.Infrastructure;
using ContratacaoService.Infrastructure.Repositories;
using ContratacaoService.Tests.TestDoubles;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ContratacaoService.Tests.Application;

public class ContratacaoUseCasesTests
{
    private static (IContratacaoUseCases uc, FakePropostaGateway fake) BuildSut()
    {
        // DB em memória
        var conn = new SqliteConnection("DataSource=:memory:");
        conn.Open();

        var options = new DbContextOptionsBuilder<ContratacaoDbContext>()
            .UseSqlite(conn)
            .Options;

        var ctx = new ContratacaoDbContext(options);
        ctx.Database.EnsureCreated();

        IContratacaoRepository repo = new ContratacaoRepository(ctx);
        var fake = new FakePropostaGateway();
        IContratacaoUseCases uc = new ContratacaoUseCases(repo, fake);
        return (uc, fake);
    }

    [Fact]
    public async Task Contratar_DeveCriar_QuandoPropostaAprovada()
    {
        var (uc, fake) = BuildSut();
        var propostaId = Guid.NewGuid();
        fake.SetStatus(propostaId, PropostaStatus.Aprovada);

        var id = await uc.ContratarAsync(propostaId);

        id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Contratar_DeveFalhar_QuandoPropostaNaoEncontrada()
    {
        var (uc, _) = BuildSut();
        var propostaId = Guid.NewGuid();

        Func<Task> act = () => uc.ContratarAsync(propostaId);
        await act.Should().ThrowAsync<InvalidOperationException>()
                 .WithMessage("*não encontrada*");
    }

    [Fact]
    public async Task Contratar_DeveFalhar_QuandoPropostaNaoAprovada()
    {
        var (uc, fake) = BuildSut();
        var propostaId = Guid.NewGuid();
        fake.SetStatus(propostaId, PropostaStatus.EmAnalise);

        Func<Task> act = () => uc.ContratarAsync(propostaId);
        await act.Should().ThrowAsync<InvalidOperationException>()
                 .WithMessage("*não está Aprovada*");
    }
}
