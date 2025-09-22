using ContratacaoService.Domain.Entities;

namespace ContratacaoService.Domain.Repositories;

public interface IContratacaoRepository
{
    Task AdicionarAsync(Contratacao contratacao, CancellationToken cancellationToken = default);
    Task<Contratacao?> ObterPorPropostaAsync(Guid propostaId, CancellationToken cancellationToken = default);
    Task<Contratacao?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
}