using Microsoft.EntityFrameworkCore;
using PropostaService.Domain.Entities;

namespace PropostaService.Infrastructure;

public class PropostaDbContext : DbContext
{
    public DbSet<Proposta> Propostas => Set<Proposta>();

    public PropostaDbContext(DbContextOptions<PropostaDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Proposta>().HasKey(p => p.Id);
    }
}
