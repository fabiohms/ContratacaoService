using ContratacaoService.Application.Commands;
using ContratacaoService.Application.DTOs;

namespace ContratacaoService.Application.Ports.Inbound;

/// <summary>
/// Port de entrada para operações de contratação
/// </summary>
public interface IContratacaoUseCase
{
    Task<ContratacaoDto> ContratarAsync(SolicitarContratacaoCommand command, CancellationToken cancellationToken = default);
    Task<ContratacaoDto?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
}