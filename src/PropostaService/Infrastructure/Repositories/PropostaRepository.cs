using Microsoft.EntityFrameworkCore;
using PropostaService.Application.Interfaces;
using PropostaService.Domain.Entities;

namespace PropostaService.Infrastructure.Repositories;

public class PropostaRepository : IPropostaRepository
{
    private readonly PropostaDbContext _context;
    public PropostaRepository(PropostaDbContext context) => _context = context;

    public async Task AddAsync(Proposta proposta, CancellationToken ct = default)
    {
        await _context.Propostas.AddAsync(proposta, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<Proposta?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _context.Propostas.FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<IReadOnlyList<Proposta>> GetAllAsync(CancellationToken ct = default) =>
        await _context.Propostas.AsNoTracking().ToListAsync(ct);

    public async Task UpdateAsync(Proposta proposta, CancellationToken ct = default)
    {
        _context.Propostas.Update(proposta);
        await _context.SaveChangesAsync(ct);
    }
}
