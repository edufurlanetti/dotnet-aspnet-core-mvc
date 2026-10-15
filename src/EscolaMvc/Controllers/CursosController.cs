using EscolaMvc.Data;
using EscolaMvc.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EscolaMvc.Controllers;

/// <summary>CRUD completo de cursos: Index, Details, Create, Edit e Delete.</summary>
public class CursosController(EscolaContext contexto) : Controller
{
    // GET: /Cursos?busca=python&area=Tecnologia
    public async Task<IActionResult> Index(string? busca, AreaCurso? area)
    {
        var consulta = contexto.Cursos.Include(c => c.Matriculas).AsNoTracking();

        if (!string.IsNullOrWhiteSpace(busca))
            consulta = consulta.Where(c => EF.Functions.Like(c.Nome, $"%{busca}%"));   // LIKE: ignora maiúsculas
        if (area is not null)
            consulta = consulta.Where(c => c.Area == area);

        ViewData["Busca"] = busca;
        ViewData["Area"] = area;
        return View(await consulta.OrderBy(c => c.DataInicio).ToListAsync());
    }

    // GET: /Cursos/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var curso = await contexto.Cursos
            .Include(c => c.Matriculas).ThenInclude(m => m.Aluno)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);

        return curso is null ? NotFound() : View(curso);
    }

    // GET: /Cursos/Create
    public IActionResult Create() =>
        View(new Curso { DataInicio = DateOnly.FromDateTime(DateTime.Today.AddMonths(1)) });

    // POST: /Cursos/Create
    // [Bind] define quais campos o formulário pode preencher (proteção contra overposting)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("Nome,Area,Descricao,CargaHoraria,Vagas,Valor,DataInicio,Ativo")] Curso curso)
    {
        if (!ModelState.IsValid)
            return View(curso);                      // devolve o formulário com as mensagens de erro

        contexto.Add(curso);
        await contexto.SaveChangesAsync();
        TempData["Mensagem"] = $"Curso \"{curso.Nome}\" cadastrado com sucesso.";
        return RedirectToAction(nameof(Index));     // padrão Post/Redirect/Get
    }

    // GET: /Cursos/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var curso = await contexto.Cursos.FindAsync(id);
        return curso is null ? NotFound() : View(curso);
    }

    // POST: /Cursos/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id,
        [Bind("Id,Nome,Area,Descricao,CargaHoraria,Vagas,Valor,DataInicio,Ativo")] Curso curso)
    {
        if (id != curso.Id)
            return BadRequest();

        var ocupadas = await contexto.Matriculas
            .CountAsync(m => m.CursoId == id && m.Status != StatusMatricula.Cancelada);
        if (curso.Vagas < ocupadas)
            ModelState.AddModelError(nameof(Curso.Vagas), $"O curso já tem {ocupadas} matrícula(s): as vagas não podem ser menores.");

        if (!ModelState.IsValid)
            return View(curso);

        contexto.Update(curso);
        await contexto.SaveChangesAsync();
        TempData["Mensagem"] = "Curso atualizado.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // GET: /Cursos/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var curso = await contexto.Cursos.Include(c => c.Matriculas).AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);
        return curso is null ? NotFound() : View(curso);
    }

    // POST: /Cursos/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var curso = await contexto.Cursos.Include(c => c.Matriculas).FirstOrDefaultAsync(c => c.Id == id);
        if (curso is null)
            return NotFound();

        if (curso.Matriculas.Count > 0)
        {
            // Com histórico de matrículas, o curso é apenas desativado
            curso.Ativo = false;
            TempData["Mensagem"] = $"\"{curso.Nome}\" possui matrículas e foi desativado em vez de excluído.";
        }
        else
        {
            contexto.Cursos.Remove(curso);
            TempData["Mensagem"] = $"Curso \"{curso.Nome}\" excluído.";
        }

        await contexto.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
