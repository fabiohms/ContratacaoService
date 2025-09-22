using System.ComponentModel.DataAnnotations;

namespace ContratacaoService.Domain.Entities;

public class Contratacao
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid PropostaId { get; set; }

    [Required]
    public DateTime DataContratacao { get; set; } = DateTime.UtcNow;
}