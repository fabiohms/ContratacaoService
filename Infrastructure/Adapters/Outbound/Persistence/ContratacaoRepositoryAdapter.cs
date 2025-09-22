using ContratacaoService.Application.Ports.Outbound;
using ContratacaoService.Domain.Entities;
using ContratacaoService.Domain.Repositories;

namespace ContratacaoService.Infrastructure.Adapters.Outbound.Persistence;

/// <summary>
/// Adapter de saída para persistência de contratações via Entity Framework
/// </summary>
public class ContratacaoRepositoryAdapter : IContratacaoRepositoryPort
{
    private readonly IContratacaoRepository _repository;

    public ContratacaoRepositoryAdapter(IContratacaoRepository repository)
    {
        _repository = repository;
    }

    public Task AdicionarAsync(Contratacao contratacao, CancellationToken cancellationToken = default)
    {
        return _repository.AdicionarAsync(contratacao, cancellationToken);
    }

    public Task<Contratacao?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _repository.ObterPorIdAsync(id, cancellationToken);
    }

    public Task<Contratacao?> ObterPorPropostaAsync(Guid propostaId, CancellationToken cancellationToken = default)
    {
        return _repository.ObterPorPropostaAsync(propostaId, cancellationToken);
    }
}