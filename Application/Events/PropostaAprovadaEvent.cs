using ContratacaoService.Domain.ValueObjects;
using System.Text.Json.Serialization;

namespace ContratacaoService.Application.Events;

public class PropostaAprovadaEvent
{
    [JsonPropertyName("PropostaId")]
    public Guid Id { get; set; }
    public string Cliente { get; set; } = string.Empty;
    public decimal Valor { get; set; }
}