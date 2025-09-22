using ContratacaoService.Domain.ValueObjects;
using System.ComponentModel.DataAnnotations;

namespace ContratacaoService.Domain.Entities;

// Entidade para armazenar status local das propostas (cache/replica)
public class PropostaStatus
{
    [Key]
    public Guid PropostaId { get; set; }
    
    [Required]
    public StatusProposta Status { get; set; }
    
    [Required]
    public DateTime UltimaAtualizacao { get; set; } = DateTime.UtcNow;
    
    public string? Motivo { get; set; } // Razão da rejeição quando Status = Reprovada
}