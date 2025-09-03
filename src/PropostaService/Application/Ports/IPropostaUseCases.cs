namespace PropostaService.Application.Ports;

using PropostaService.Domain.Entities;

public interface IPropostaUseCases
{
    Task<Guid> CriarPropostaAsync(string nomeCliente, decimal valor, CancellationToken ct = default);
    Task<IReadOnlyList<Proposta>> ListarPropostasAsync(CancellationToken ct = default);
    Task AlterarStatusAsync(Guid id, PropostaStatus status, CancellationToken ct = default);
}
