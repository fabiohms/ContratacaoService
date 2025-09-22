using ContratacaoService.Application.Commands;
using ContratacaoService.Application.DTOs;
using ContratacaoService.Application.Ports.Inbound;
using Microsoft.AspNetCore.Mvc;

namespace ContratacaoService.Infrastructure.Adapters.Inbound.Rest;

/// <summary>
/// Adapter de entrada REST para contratações
/// </summary>
[ApiController]
[Route("api/contratacoes")]
public class ContratacoesController : ControllerBase
{
    private readonly IContratacaoUseCase _contratacaoUseCase;

    public ContratacoesController(IContratacaoUseCase contratacaoUseCase)
    {
        _contratacaoUseCase = contratacaoUseCase;
    }

    [HttpPost]
    public async Task<ActionResult<ContratacaoDto>> Post([FromBody] SolicitarContratacaoCommand command, CancellationToken ct)
    {
        try
        {
            var result = await _contratacaoUseCase.ContratarAsync(command, ct);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ContratacaoDto>> GetById(Guid id, CancellationToken ct)
    {
        var result = await _contratacaoUseCase.ObterPorIdAsync(id, ct);
        if (result is null) return NotFound();
        return Ok(result);
    }
}