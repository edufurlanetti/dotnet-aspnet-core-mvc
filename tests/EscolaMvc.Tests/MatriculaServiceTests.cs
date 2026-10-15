using EscolaMvc.Data;
using EscolaMvc.Models;
using EscolaMvc.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace EscolaMvc.Tests;

/// <summary>
/// Testa as regras de matrícula contra um SQLite REAL em memória:
/// as consultas do EF Core são de fato traduzidas e executadas em SQL.
/// Cada teste recebe um banco novo (o xUnit cria uma instância da classe por teste).
/// </summary>
public sealed class MatriculaServiceTests : IDisposable
{
    private readonly SqliteConnection _conexao = new("DataSource=:memory:");
    private readonly EscolaContext _contexto;
    private readonly MatriculaService _servico;

    // Ids gerados na ordem de inserção
    private const int CursoDuasVagas = 1, CursoInativo = 2;
    private const int Ana = 1, Bruno = 2, Carla = 3;

    public MatriculaServiceTests()
    {
        _conexao.Open();   // o banco em memória existe enquanto a conexão estiver aberta

        var opcoes = new DbContextOptionsBuilder<EscolaContext>().UseSqlite(_conexao).Options;
        _contexto = new EscolaContext(opcoes);
        _contexto.Database.EnsureCreated();

        _contexto.Cursos.AddRange(
            new Curso { Nome = "Curso com duas vagas", CargaHoraria = 10, Vagas = 2, Valor = 100m, DataInicio = new DateOnly(2026, 12, 1) },
            new Curso { Nome = "Curso desativado", CargaHoraria = 10, Vagas = 10, Valor = 0m, DataInicio = new DateOnly(2026, 12, 1), Ativo = false });
        _contexto.Alunos.AddRange(
            new Aluno { Nome = "Ana", Email = "ana@teste.com" },
            new Aluno { Nome = "Bruno", Email = "bruno@teste.com" },
            new Aluno { Nome = "Carla", Email = "carla@teste.com" });
        _contexto.SaveChanges();

        _servico = new MatriculaService(_contexto);
    }

    public void Dispose()
    {
        _contexto.Dispose();
        _conexao.Dispose();
    }

    [Fact]
    public async Task Matricula_aluno_quando_ha_vaga()
    {
        var resultado = await _servico.MatricularAsync(Ana, CursoDuasVagas);

        Assert.True(resultado.Sucesso);
        Assert.Equal(1, await _contexto.Matriculas.CountAsync());
    }

    [Fact]
    public async Task Nao_matricula_o_mesmo_aluno_duas_vezes()
    {
        await _servico.MatricularAsync(Ana, CursoDuasVagas);

        var resultado = await _servico.MatricularAsync(Ana, CursoDuasVagas);

        Assert.False(resultado.Sucesso);
        Assert.Contains("já está matriculado", resultado.Erro);
    }

    [Fact]
    public async Task Respeita_o_limite_de_vagas()
    {
        await _servico.MatricularAsync(Ana, CursoDuasVagas);
        await _servico.MatricularAsync(Bruno, CursoDuasVagas);

        var resultado = await _servico.MatricularAsync(Carla, CursoDuasVagas);

        Assert.False(resultado.Sucesso);
        Assert.Contains("Não há vagas", resultado.Erro);
    }

    [Fact]
    public async Task Cancelamento_libera_a_vaga()
    {
        await _servico.MatricularAsync(Ana, CursoDuasVagas);
        await _servico.MatricularAsync(Bruno, CursoDuasVagas);
        var matriculaDaAna = await _contexto.Matriculas.FirstAsync(m => m.AlunoId == Ana);

        await _servico.CancelarAsync(matriculaDaAna.Id);
        var resultado = await _servico.MatricularAsync(Carla, CursoDuasVagas);

        Assert.True(resultado.Sucesso);
    }

    [Fact]
    public async Task Curso_desativado_nao_aceita_matricula()
    {
        var resultado = await _servico.MatricularAsync(Ana, CursoInativo);

        Assert.False(resultado.Sucesso);
        Assert.Contains("não está recebendo", resultado.Erro);
    }

    [Fact]
    public async Task Conclusao_registra_a_nota()
    {
        await _servico.MatricularAsync(Ana, CursoDuasVagas);
        var matricula = await _contexto.Matriculas.FirstAsync();

        var resultado = await _servico.ConcluirAsync(matricula.Id, 8.5m);

        Assert.True(resultado.Sucesso);
        Assert.Equal(StatusMatricula.Concluida, matricula.Status);
        Assert.Equal(8.5m, matricula.NotaFinal);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(10.5)]
    public async Task Rejeita_nota_fora_do_intervalo(double nota)
    {
        await _servico.MatricularAsync(Ana, CursoDuasVagas);
        var matricula = await _contexto.Matriculas.FirstAsync();

        var resultado = await _servico.ConcluirAsync(matricula.Id, (decimal)nota);

        Assert.False(resultado.Sucesso);
    }

    [Fact]
    public async Task Nao_conclui_matricula_cancelada()
    {
        await _servico.MatricularAsync(Ana, CursoDuasVagas);
        var matricula = await _contexto.Matriculas.FirstAsync();
        await _servico.CancelarAsync(matricula.Id);

        var resultado = await _servico.ConcluirAsync(matricula.Id, 7m);

        Assert.False(resultado.Sucesso);
    }
}
