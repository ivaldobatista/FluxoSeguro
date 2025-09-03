using ContratacaoService.Domain.Entities;
using FluentAssertions;

namespace ContratacaoService.Tests.Domain;

public class ContratacaoTests
{
    [Fact]
    public void NovaContratacao_DevePreencher_Id_PropostaId_Data()
    {
        var propostaId = Guid.NewGuid();
        var c = new Contratacao(propostaId, DateTimeOffset.UtcNow);

        c.Id.Should().NotBeEmpty();
        c.PropostaId.Should().Be(propostaId);
        c.DataContratacao.Should().BeAfter(DateTimeOffset.UtcNow.AddMinutes(-1));
    }

    [Fact]
    public void NovaContratacao_ComPropostaVazia_DeveFalhar()
    {
        Action act = () => new Contratacao(Guid.Empty, DateTimeOffset.UtcNow);
        act.Should().Throw<ArgumentException>();
    }
}
