using ContratacaoService.Domain.ValueObjects;
using System.Text.Json.Serialization;

namespace ContratacaoService.Application.Events;

public class PropostaRejeitadaEvent
{
    [JsonPropertyName("PropostaId")]
    public Guid Id { get; set; }
    public string Cliente { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public string Motivo { get; set; } = string.Empty;
}