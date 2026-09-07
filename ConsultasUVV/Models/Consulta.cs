using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ConsultasUVV.Models;

public class Consulta
{
    public int Id { get; set; }

    [Required(ErrorMessage = "A especialidade é obrigatória.")]
    [StringLength(100, ErrorMessage = "A especialidade deve ter no máximo 100 caracteres.")]
    [Display(Name = "Especialidade")]
    public string Especialidade { get; set; } = string.Empty;

    [Required(ErrorMessage = "A data e hora são obrigatórias.")]
    [DataType(DataType.DateTime)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
    [Display(Name = "Data / Hora")]
    public DateTime DataHora { get; set; } = DateTime.Now;

    [Required(ErrorMessage = "A descrição é obrigatória.")]
    [StringLength(500, ErrorMessage = "A descrição deve ter no máximo 500 caracteres.")]
    [Display(Name = "Descrição")]
    public string Descricao { get; set; } = string.Empty;

    [Display(Name = "Usuário")]
    public int UsuarioId { get; set; }

    [ForeignKey(nameof(UsuarioId))]
    [ValidateNever]
    public Usuario? Usuario { get; set; }
}
