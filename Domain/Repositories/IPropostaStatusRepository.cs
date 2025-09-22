using ContratacaoService.Domain.Entities;
using ContratacaoService.Domain.ValueObjects;

namespace ContratacaoService.Domain.Repositories;

public interface IPropostaStatusRepository
{
    Task ArmazenarStatusAsync(Guid propostaId, StatusProposta status, string? motivo = null, CancellationToken cancellationToken = default);
    Task<StatusProposta?> ObterStatusAsync(Guid propostaId, CancellationToken cancellationToken = default);
    Task<PropostaStatus?> ObterPropostaStatusAsync(Guid propostaId, CancellationToken cancellationToken = default);
}