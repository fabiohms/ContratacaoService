using ContratacaoService.Domain.Entities;
using ContratacaoService.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ContratacaoService.Infrastructure;

public class ContratacaoRepository : IContratacaoRepository
{
    private readonly ContratacaoDbContext _db;

    public ContratacaoRepository(ContratacaoDbContext db)
    {
        _db = db;
    }

    public async Task AdicionarAsync(Contratacao contratacao, CancellationToken cancellationToken = default)
    {
        _db.Contratacoes.Add(contratacao);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public Task<Contratacao?> ObterPorPropostaAsync(Guid propostaId, CancellationToken cancellationToken = default)
    {
        return _db.Contratacoes.AsNoTracking().FirstOrDefaultAsync(c => c.PropostaId == propostaId, cancellationToken);
    }

    public Task<Contratacao?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _db.Contratacoes.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }
}
