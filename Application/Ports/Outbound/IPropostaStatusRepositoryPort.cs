using ContratacaoService.Domain.Entities;
using ContratacaoService.Domain.ValueObjects;

namespace ContratacaoService.Application.Ports.Outbound;

/// <summary>
/// Port de saída para cache de status de propostas
/// </summary>
public interface IPropostaStatusRepositoryPort
{
    Task ArmazenarStatusAsync(Guid propostaId, StatusProposta status, string? motivo = null, CancellationToken cancellationToken = default);
    Task<StatusProposta?> ObterStatusAsync(Guid propostaId, CancellationToken cancellationToken = default);
    Task<PropostaStatus?> ObterPropostaStatusAsync(Guid propostaId, CancellationToken cancellationToken = default);
}