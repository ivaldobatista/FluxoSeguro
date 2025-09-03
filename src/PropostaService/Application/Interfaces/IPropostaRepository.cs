using PropostaService.Domain.Entities;

namespace PropostaService.Application.Interfaces;

public interface IPropostaRepository
{
    Task<Proposta?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Proposta proposta, CancellationToken ct = default);
    Task UpdateAsync(Proposta proposta, CancellationToken ct = default);
    Task<IReadOnlyList<Proposta>> GetAllAsync(CancellationToken ct = default);
}
