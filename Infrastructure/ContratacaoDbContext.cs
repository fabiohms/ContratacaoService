using ContratacaoService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ContratacaoService.Infrastructure;

/// <summary>
/// DbContext para contratações - usa o mesmo banco do PropostaService
/// PropostaStatus removido - não deve ser persistido fisicamente
/// </summary>
public class ContratacaoDbContext : DbContext
{
    public ContratacaoDbContext(DbContextOptions<ContratacaoDbContext> options) : base(options) { }

    public DbSet<Contratacao> Contratacoes => Set<Contratacao>();
    // PropostasStatus removido - cache apenas em memória

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Contratacao>(b =>
        {
            b.ToTable("Contratacoes");  // Remove schema, use default
            b.HasKey(x => x.Id);
            b.HasIndex(x => x.PropostaId).IsUnique();
        });

        // PropostaStatus removido - não persiste no banco
    }
}
