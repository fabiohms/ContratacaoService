using ContratacaoService.Application.Commands;
using ContratacaoService.Application.DTOs;
using ContratacaoService.Application.Ports.Inbound;
using ContratacaoService.Application.Ports.Outbound;
using ContratacaoService.Domain.Entities;
using ContratacaoService.Domain.ValueObjects;

namespace ContratacaoService.Application.UseCases;

/// <summary>
/// Implementação dos casos de uso de contratação
/// </summary>
public class ContratacaoUseCase : IContratacaoUseCase
{
    private readonly IPropostaServicePort _propostaServicePort;
    private readonly IContratacaoRepositoryPort _contratacaoRepositoryPort;

    public ContratacaoUseCase(
        IPropostaServicePort propostaServicePort, 
        IContratacaoRepositoryPort contratacaoRepositoryPort)
    {
        _propostaServicePort = propostaServicePort;
        _contratacaoRepositoryPort = contratacaoRepositoryPort;
    }

    public async Task<ContratacaoDto> ContratarAsync(SolicitarContratacaoCommand command, CancellationToken cancellationToken = default)
    {
        // Verifica se já existe contratação para a proposta
        var existente = await _contratacaoRepositoryPort.ObterPorPropostaAsync(command.PropostaId, cancellationToken);
        if (existente is not null)
        {
            return new ContratacaoDto(existente.Id, existente.PropostaId, existente.DataContratacao);
        }

        var status = await _propostaServicePort.ObterStatusAsync(command.PropostaId, cancellationToken);
        if (status is null)
        {
            throw new InvalidOperationException("Proposta não encontrada no PropostaService.");
        }
        if (status != StatusProposta.Aprovada)
        {
            throw new InvalidOperationException("Só é possível contratar propostas aprovadas.");
        }

        var contratacao = new Contratacao
        {
            PropostaId = command.PropostaId,
            DataContratacao = DateTime.UtcNow
        };

        await _contratacaoRepositoryPort.AdicionarAsync(contratacao, cancellationToken);

        return new ContratacaoDto(contratacao.Id, contratacao.PropostaId, contratacao.DataContratacao);
    }

    public async Task<ContratacaoDto?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _contratacaoRepositoryPort.ObterPorIdAsync(id, cancellationToken);
        return entity is null ? null : new ContratacaoDto(entity.Id, entity.PropostaId, entity.DataContratacao);
    }
}