using ContratacaoService.Domain.Entities;

namespace ContratacaoService.Application.Ports;

public interface IContratacaoUseCases
{
    Task<Guid> ContratarAsync(Guid propostaId, CancellationToken ct = default);
    Task<IReadOnlyList<Contratacao>> ListarAsync(CancellationToken ct = default);
}
