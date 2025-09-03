namespace ContratacaoService.Application.Ports;

public enum PropostaStatus { EmAnalise = 0, Aprovada = 1, Rejeitada = 2 }

public interface IPropostaGateway
{
    Task<PropostaStatus?> ObterStatusAsync(Guid propostaId, CancellationToken ct = default);
}
