using ContratacaoService.Domain.Entities;

namespace ContratacaoService.Application.Ports.Outbound;

/// <summary>
/// Port de saída para persistência de contratações
/// </summary>
public interface IContratacaoRepositoryPort
{
    Task AdicionarAsync(Contratacao contratacao, CancellationToken cancellationToken = default);
    Task<Contratacao?> ObterPorPropostaAsync(Guid propostaId, CancellationToken cancellationToken = default);
    Task<Contratacao?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
}