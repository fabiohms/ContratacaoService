using ContratacaoService.Domain.ValueObjects;

namespace ContratacaoService.Application.Ports.Outbound;

/// <summary>
/// Port de saída para obter informações de propostas
/// </summary>
public interface IPropostaServicePort
{
    Task<StatusProposta?> ObterStatusAsync(Guid propostaId, CancellationToken cancellationToken = default);
}