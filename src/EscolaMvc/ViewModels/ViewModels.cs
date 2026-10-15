using System.ComponentModel.DataAnnotations;
using EscolaMvc.Models;

namespace EscolaMvc.ViewModels;

/// <summary>Dados exibidos no painel inicial.</summary>
public record PainelViewModel(
    int TotalCursos,
    int TotalAlunos,
    int MatriculasAtivas,
    decimal ReceitaPrevista,
    IReadOnlyList<Curso> CursosComPoucasVagas);

/// <summary>Formulário de nova matrícula (só o que a tela precisa).</summary>
public class NovaMatriculaViewModel
{
    [Required(ErrorMessage = "Selecione o aluno")]
    [Display(Name = "Aluno")]
    public int? AlunoId { get; set; }

    [Required(ErrorMessage = "Selecione o curso")]
    [Display(Name = "Curso")]
    public int? CursoId { get; set; }
}
