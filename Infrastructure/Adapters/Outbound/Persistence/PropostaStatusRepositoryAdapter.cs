using ContratacaoService.Application.Ports.Outbound;
using ContratacaoService.Domain.Repositories;
using ContratacaoService.Domain.Entities;
using ContratacaoService.Domain.ValueObjects;

namespace ContratacaoService.Infrastructure.Adapters.Outbound.Persistence;

/// <summary>
/// Adapter de saída para cache em memória de status de propostas
/// </summary>
public class PropostaStatusRepositoryAdapter : IPropostaStatusRepositoryPort
{
    private readonly IPropostaStatusRepository _repository;

    public PropostaStatusRepositoryAdapter(IPropostaStatusRepository repository)
    {
        _repository = repository;
    }

    public Task ArmazenarStatusAsync(Guid propostaId, StatusProposta status, string? motivo = null, CancellationToken cancellationToken = default)
    {
        return _repository.ArmazenarStatusAsync(propostaId, status, motivo, cancellationToken);
    }

    public Task<StatusProposta?> ObterStatusAsync(Guid propostaId, CancellationToken cancellationToken = default)
    {
        return _repository.ObterStatusAsync(propostaId, cancellationToken);
    }

    public Task<PropostaStatus?> ObterPropostaStatusAsync(Guid propostaId, CancellationToken cancellationToken = default)
    {
        return _repository.ObterPropostaStatusAsync(propostaId, cancellationToken);
    }
}