using System.ComponentModel.DataAnnotations;
using ConsultasUVV.Validation;

namespace ConsultasUVV.Models.ViewModels;

public class EsqueciSenhaViewModel
{
    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    [Display(Name = "E-mail")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "O CPF é obrigatório.")]
    [Cpf]
    [Display(Name = "CPF")]
    public string Cpf { get; set; } = string.Empty;
}
