using System.ComponentModel.DataAnnotations;

namespace EscolaMvc.Models;

public class Aluno
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Informe o nome")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 100 caracteres")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o e-mail")]
    [EmailAddress(ErrorMessage = "E-mail inválido")]
    [StringLength(120)]
    [Display(Name = "E-mail")]
    public string Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Telefone inválido")]
    [StringLength(20)]
    public string? Telefone { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Nascimento")]
    public DateOnly? DataNascimento { get; set; }

    public ICollection<Matricula> Matriculas { get; set; } = new List<Matricula>();
}
