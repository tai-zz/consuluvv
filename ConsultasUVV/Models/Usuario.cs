using System.ComponentModel.DataAnnotations;
using ConsultasUVV.Validation;

namespace ConsultasUVV.Models;

public class Usuario
{
    public int Id { get; set; }

    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 100 caracteres.")]
    [Display(Name = "Nome")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    [StringLength(150)]
    [Display(Name = "E-mail")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "O CPF é obrigatório.")]
    [Cpf]
    [StringLength(11)]
    [Display(Name = "CPF")]
    public string Cpf { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Senha")]
    public string Senha { get; set; } = string.Empty;

    [Display(Name = "Data de Cadastro")]
    [DataType(DataType.DateTime)]
    public DateTime DataCadastro { get; set; } = DateTime.Now;

    public string? TokenResetSenha { get; set; }

    public DateTime? TokenResetExpiraEm { get; set; }

    public ICollection<Consulta> Consultas { get; set; } = new List<Consulta>();

    public ICollection<Notificacao> Notificacoes { get; set; } = new List<Notificacao>();
}
