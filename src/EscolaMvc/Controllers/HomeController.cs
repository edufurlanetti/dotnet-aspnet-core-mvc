using System.Diagnostics;
using EscolaMvc.Data;
using EscolaMvc.Models;
using EscolaMvc.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EscolaMvc.Controllers;

public class HomeController(EscolaContext contexto) : Controller
{
    public async Task<IActionResult> Index()
    {
        var cursos = await contexto.Cursos
            .Include(c => c.Matriculas)
            .AsNoTracking()                     // somente leitura: dispensa o rastreamento de alterações
            .ToListAsync();

        var matriculasAtivas = cursos.Sum(c => c.Matriculas.Count(m => m.Status == StatusMatricula.Ativa));

        // O SQLite não soma decimal no servidor: o cálculo é feito em memória
        var receita = cursos.Sum(c => c.Valor * c.Matriculas.Count(m => m.Status != StatusMatricula.Cancelada));

        var painel = new PainelViewModel(
            TotalCursos: cursos.Count(c => c.Ativo),
            TotalAlunos: await contexto.Alunos.CountAsync(),
            MatriculasAtivas: matriculasAtivas,
            ReceitaPrevista: receita,
            CursosComPoucasVagas: cursos.Where(c => c.Ativo && c.VagasLivres <= 3)
                                        .OrderBy(c => c.VagasLivres).ToList());

        return View(painel);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(model: Activity.Current?.Id ?? HttpContext.TraceIdentifier);
}
