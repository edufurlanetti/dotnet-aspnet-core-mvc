using System.ComponentModel.DataAnnotations;

namespace EscolaMvc.Models;

public enum StatusMatricula
{
    Ativa,
    [Display(Name = "Concluída")] Concluida,
    Cancelada
}

/// <summary>Entidade associativa entre Aluno e Curso (relacionamento N:N com dados próprios).</summary>
public class Matricula
{
    public int Id { get; set; }

    public int CursoId { get; set; }
    public Curso? Curso { get; set; }

    public int AlunoId { get; set; }
    public Aluno? Aluno { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Data")]
    public DateTime DataMatricula { get; set; } = DateTime.Now;

    public StatusMatricula Status { get; set; } = StatusMatricula.Ativa;

    [Range(0, 10)]
    [Display(Name = "Nota final")]
    public decimal? NotaFinal { get; set; }
}
