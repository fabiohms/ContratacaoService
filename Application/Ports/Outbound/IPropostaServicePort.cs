using ContratacaoService.Domain.ValueObjects;

namespace ContratacaoService.Application.Ports.Outbound;

/// <summary>
/// Port de sa�da para obter informa��es de propostas
/// </summary>
public interface IPropostaServicePort
{
    Task<StatusProposta?> ObterStatusAsync(Guid propostaId, CancellationToken cancellationToken = default);
}