using ContratacaoService.Application.Interfaces;
using ContratacaoService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ContratacaoService.Infrastructure.Repositories;

public sealed class ContratacaoRepository : IContratacaoRepository
{
    private readonly ContratacaoDbContext _ctx;
    public ContratacaoRepository(ContratacaoDbContext ctx) => _ctx = ctx;

    public async Task AddAsync(Contratacao entity, CancellationToken ct = default)
    {
        _ctx.Contratacoes.Add(entity);
        await _ctx.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<Contratacao>> GetAllAsync(CancellationToken ct = default)
    {
        var data = await _ctx.Contratacoes.AsNoTracking().ToListAsync(ct);
        return data.OrderByDescending(x => x.DataContratacao).ToList(); 
    }
}
