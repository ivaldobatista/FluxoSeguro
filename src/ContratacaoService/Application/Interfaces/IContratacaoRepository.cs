using ContratacaoService.Domain.Entities;

namespace ContratacaoService.Application.Interfaces;

public interface IContratacaoRepository
{
    Task AddAsync(Contratacao entity, CancellationToken ct = default);
    Task<IReadOnlyList<Contratacao>> GetAllAsync(CancellationToken ct = default);
}
