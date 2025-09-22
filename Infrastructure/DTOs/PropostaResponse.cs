using ContratacaoService.Domain.ValueObjects;

namespace ContratacaoService.Infrastructure.DTOs;

public record PropostaResponse(
    Guid Id,
    string Cliente,
    List<CoberturaResponse> Coberturas,
    decimal Valor,
    StatusProposta Status,
    string? Motivo,
    DateTime CreatedAt
);

public record CoberturaResponse(string Nome, decimal Valor);