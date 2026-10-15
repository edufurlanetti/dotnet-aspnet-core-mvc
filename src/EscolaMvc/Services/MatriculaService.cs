using EscolaMvc.Data;
using EscolaMvc.Models;
using Microsoft.EntityFrameworkCore;

namespace EscolaMvc.Services;

/// <summary>Resultado de uma operação: sucesso ou mensagem de erro para exibir na tela.</summary>
public record Resultado(bool Sucesso, string? Erro = null)
{
    public static Resultado Ok() => new(true);
    public static Resultado Falha(string erro) => new(false, erro);
}

/// <summary>
/// Regras de negócio das matrículas, fora do controller: assim podem ser
/// testadas isoladamente e reutilizadas (ex.: por uma futura API).
/// </summary>
public class MatriculaService(EscolaContext contexto)
{
    public async Task<Resultado> MatricularAsync(int alunoId, int cursoId)
    {
        var curso = await contexto.Cursos
            .Include(c => c.Matriculas)
            .FirstOrDefaultAsync(c => c.Id == cursoId);

        if (curso is null)
            return Resultado.Falha("Curso não encontrado.");
        if (!curso.Ativo)
            return Resultado.Falha("O curso não está recebendo matrículas.");
        if (curso.VagasLivres <= 0)
            return Resultado.Falha($"Não há vagas em \"{curso.Nome}\".");
        if (!await contexto.Alunos.AnyAsync(a => a.Id == alunoId))
            return Resultado.Falha("Aluno não encontrado.");

        var jaMatriculado = curso.Matriculas.Any(m => m.AlunoId == alunoId && m.Status != StatusMatricula.Cancelada);
        if (jaMatriculado)
            return Resultado.Falha("O aluno já está matriculado neste curso.");

        contexto.Matriculas.Add(new Matricula { AlunoId = alunoId, CursoId = cursoId });
        await contexto.SaveChangesAsync();
        return Resultado.Ok();
    }

    public async Task<Resultado> CancelarAsync(int matriculaId)
    {
        var matricula = await contexto.Matriculas.FindAsync(matriculaId);
        if (matricula is null)
            return Resultado.Falha("Matrícula não encontrada.");
        if (matricula.Status != StatusMatricula.Ativa)
            return Resultado.Falha("Somente matrículas ativas podem ser canceladas.");

        matricula.Status = StatusMatricula.Cancelada;
        await contexto.SaveChangesAsync();
        return Resultado.Ok();
    }

    public async Task<Resultado> ConcluirAsync(int matriculaId, decimal nota)
    {
        if (nota is < 0 or > 10)
            return Resultado.Falha("A nota deve estar entre 0 e 10.");

        var matricula = await contexto.Matriculas.FindAsync(matriculaId);
        if (matricula is null)
            return Resultado.Falha("Matrícula não encontrada.");
        if (matricula.Status != StatusMatricula.Ativa)
            return Resultado.Falha("Somente matrículas ativas podem ser concluídas.");

        matricula.Status = StatusMatricula.Concluida;
        matricula.NotaFinal = nota;
        await contexto.SaveChangesAsync();
        return Resultado.Ok();
    }
}
