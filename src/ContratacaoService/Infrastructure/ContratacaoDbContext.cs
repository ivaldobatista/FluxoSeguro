using ContratacaoService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ContratacaoService.Infrastructure;

public sealed class ContratacaoDbContext : DbContext
{
    public ContratacaoDbContext(DbContextOptions<ContratacaoDbContext> options) : base(options) { }
    public DbSet<Contratacao> Contratacoes => Set<Contratacao>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Contratacao>(b =>
        {
            b.ToTable("Contratacoes");
            b.HasKey(x => x.Id);
            b.Property(x => x.PropostaId).IsRequired();
            b.Property(x => x.DataContratacao).IsRequired();
            b.HasIndex(x => x.PropostaId);
        });
    }
}
