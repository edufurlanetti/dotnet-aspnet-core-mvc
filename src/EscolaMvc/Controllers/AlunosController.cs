using EscolaMvc.Data;
using EscolaMvc.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EscolaMvc.Controllers;

public class AlunosController(EscolaContext contexto) : Controller
{
    public async Task<IActionResult> Index(string? busca)
    {
        var consulta = contexto.Alunos.Include(a => a.Matriculas).AsNoTracking();
        if (!string.IsNullOrWhiteSpace(busca))
            consulta = consulta.Where(a => EF.Functions.Like(a.Nome, $"%{busca}%") || EF.Functions.Like(a.Email, $"%{busca}%"));

        ViewData["Busca"] = busca;
        return View(await consulta.OrderBy(a => a.Nome).ToListAsync());
    }

    public async Task<IActionResult> Details(int id)
    {
        var aluno = await contexto.Alunos
            .Include(a => a.Matriculas).ThenInclude(m => m.Curso)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);
        return aluno is null ? NotFound() : View(aluno);
    }

    public IActionResult Create() => View(new Aluno());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Nome,Email,Telefone,DataNascimento")] Aluno aluno)
    {
        await ValidarEmailUnicoAsync(aluno);
        if (!ModelState.IsValid)
            return View(aluno);

        aluno.Email = aluno.Email.Trim().ToLowerInvariant();
        contexto.Add(aluno);
        await contexto.SaveChangesAsync();
        TempData["Mensagem"] = $"Aluno(a) {aluno.Nome} cadastrado(a).";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var aluno = await contexto.Alunos.FindAsync(id);
        return aluno is null ? NotFound() : View(aluno);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Nome,Email,Telefone,DataNascimento")] Aluno aluno)
    {
        if (id != aluno.Id)
            return BadRequest();

        await ValidarEmailUnicoAsync(aluno);
        if (!ModelState.IsValid)
            return View(aluno);

        aluno.Email = aluno.Email.Trim().ToLowerInvariant();
        contexto.Update(aluno);
        await contexto.SaveChangesAsync();
        TempData["Mensagem"] = "Dados do aluno atualizados.";
        return RedirectToAction(nameof(Details), new { id });
    }

    /// <summary>Validação que depende do banco: não cabe em um atributo.</summary>
    private async Task ValidarEmailUnicoAsync(Aluno aluno)
    {
        if (string.IsNullOrWhiteSpace(aluno.Email))
            return;

        var email = aluno.Email.Trim().ToLowerInvariant();
        if (await contexto.Alunos.AnyAsync(a => a.Email == email && a.Id != aluno.Id))
            ModelState.AddModelError(nameof(Aluno.Email), "Já existe um aluno com este e-mail.");
    }
}
