using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ConsultasUVV.Models;

public class Notificacao
{
    public int Id { get; set; }

    [Required]
    [StringLength(300)]
    public string Mensagem { get; set; } = string.Empty;

    public DateTime CriadaEm { get; set; } = DateTime.Now;

    public bool Lida { get; set; }

    public int UsuarioId { get; set; }

    [ForeignKey(nameof(UsuarioId))]
    [ValidateNever]
    public Usuario? Usuario { get; set; }

    public int? ConsultaId { get; set; }

    [ForeignKey(nameof(ConsultaId))]
    [ValidateNever]
    public Consulta? Consulta { get; set; }
}
