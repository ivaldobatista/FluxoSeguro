using FluentAssertions;
using PropostaService.Domain.Entities;

namespace PropostaService.Tests.Domain;

public class PropostaTests
{
    [Fact]
    public void NovaProposta_DeveIniciar_EmAnalise()
    {
        var p = new Proposta("Maria", 1500m);
        p.Status.Should().Be(PropostaStatus.EmAnalise);
        p.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void AlterarStatus_DeveAtualizar()
    {
        var p = new Proposta("João", 900m);
        p.AlterarStatus(PropostaStatus.Aprovada);
        p.Status.Should().Be(PropostaStatus.Aprovada);
    }
}
