using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EscolaMvc.Models;

public enum AreaCurso
{
    [Display(Name = "Tecnologia")] Tecnologia,
    [Display(Name = "Engenharia")] Engenharia,
    [Display(Name = "Arquitetura")] Arquitetura,
    [Display(Name = "Gestão")] Gestao
}

/// <summary>Curso de extensão oferecido pela faculdade.</summary>
public class Curso
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Informe o nome do curso")]
    [StringLength(100, MinimumLength = 5, ErrorMessage = "O nome deve ter entre 5 e 100 caracteres")]
    public string Nome { get; set; } = string.Empty;

    [Display(Name = "Área")]
    public AreaCurso Area { get; set; }

    [StringLength(500)]
    [Display(Name = "Descrição")]
    public string? Descricao { get; set; }

    [Range(4, 360, ErrorMessage = "A carga horária deve estar entre 4 e 360 horas")]
    [Display(Name = "Carga horária (h)")]
    public int CargaHoraria { get; set; }

    [Range(1, 200, ErrorMessage = "As vagas devem estar entre 1 e 200")]
    public int Vagas { get; set; }

    [Range(0, 10000, ErrorMessage = "Informe um valor entre R$ 0 e R$ 10.000")]
    [DataType(DataType.Currency)]
    [Column(TypeName = "decimal(10,2)")]
    public decimal Valor { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Início")]
    public DateOnly DataInicio { get; set; }

    public bool Ativo { get; set; } = true;

    public ICollection<Matricula> Matriculas { get; set; } = new List<Matricula>();

    [NotMapped]
    public int VagasOcupadas => Matriculas.Count(m => m.Status != StatusMatricula.Cancelada);

    [NotMapped]
    [Display(Name = "Vagas livres")]
    public int VagasLivres => Vagas - VagasOcupadas;
}
