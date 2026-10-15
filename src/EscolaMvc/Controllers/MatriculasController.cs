using EscolaMvc.Data;
using EscolaMvc.Models;
using EscolaMvc.Services;
using EscolaMvc.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EscolaMvc.Controllers;

public class MatriculasController(EscolaContext contexto, MatriculaService servico) : Controller
{
    public async Task<IActionResult> Index(StatusMatricula? status)
    {
        var consulta = contexto.Matriculas
            .Include(m => m.Aluno)
            .Include(m => m.Curso)
            .AsNoTracking();

        if (status is not null)
            consulta = consulta.Where(m => m.Status == status);

        ViewData["Status"] = status;
        return View(await consulta.OrderByDescending(m => m.DataMatricula).ToListAsync());
    }

    public async Task<IActionResult> Create(int? cursoId)
    {
        await CarregarListasAsync();
        return View(new NovaMatriculaViewModel { CursoId = cursoId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(NovaMatriculaViewModel formulario)
    {
        if (ModelState.IsValid)
        {
            var resultado = await servico.MatricularAsync(formulario.AlunoId!.Value, formulario.CursoId!.Value);
            if (resultado.Sucesso)
            {
                TempData["Mensagem"] = "Matrícula realizada com sucesso.";
                return RedirectToAction(nameof(Index));
            }
            ModelState.AddModelError(string.Empty, resultado.Erro!);   // erro geral do formulário
        }

        await CarregarListasAsync();
        return View(formulario);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancelar(int id)
    {
        var resultado = await servico.CancelarAsync(id);
        TempData[resultado.Sucesso ? "Mensagem" : "Erro"] = resultado.Sucesso ? "Matrícula cancelada." : resultado.Erro;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Concluir(int id, decimal nota)
    {
        var resultado = await servico.ConcluirAsync(id, nota);
        TempData[resultado.Sucesso ? "Mensagem" : "Erro"] = resultado.Sucesso ? "Matrícula concluída." : resultado.Erro;
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Preenche as listas de seleção (dropdowns) do formulário.</summary>
    private async Task CarregarListasAsync()
    {
        var alunos = await contexto.Alunos.AsNoTracking().OrderBy(a => a.Nome).ToListAsync();
        var cursos = await contexto.Cursos.Include(c => c.Matriculas).AsNoTracking()
            .Where(c => c.Ativo).OrderBy(c => c.Nome).ToListAsync();

        ViewBag.Alunos = new SelectList(alunos, nameof(Aluno.Id), nameof(Aluno.Nome));
        ViewBag.Cursos = cursos.Select(c => new SelectListItem
        {
            Value = c.Id.ToString(),
            Text = $"{c.Nome} ({c.VagasLivres} vaga(s))",
            Disabled = c.VagasLivres <= 0
        }).ToList();
    }
}
