using EscolaMvc.Models;

namespace EscolaMvc.Data;

/// <summary>Popula o banco com dados de exemplo na primeira execução.</summary>
public static class DadosIniciais
{
    public static void Popular(EscolaContext contexto)
    {
        if (contexto.Cursos.Any())
            return;

        var cursos = new[]
        {
            new Curso { Nome = "Python para Análise de Dados", Area = AreaCurso.Tecnologia, CargaHoraria = 40, Vagas = 30, Valor = 390m,
                        DataInicio = new DateOnly(2026, 8, 3), Descricao = "Pandas, visualização e introdução ao aprendizado de máquina." },
            new Curso { Nome = "Banco de Dados com SQL", Area = AreaCurso.Tecnologia, CargaHoraria = 32, Vagas = 25, Valor = 320m,
                        DataInicio = new DateOnly(2026, 8, 10), Descricao = "Modelagem, normalização e consultas SQL." },
            new Curso { Nome = "Automação com CLP", Area = AreaCurso.Engenharia, CargaHoraria = 60, Vagas = 3, Valor = 650m,
                        DataInicio = new DateOnly(2026, 9, 1), Descricao = "Programação Ladder e supervisórios." },
            new Curso { Nome = "BIM com Revit", Area = AreaCurso.Arquitetura, CargaHoraria = 48, Vagas = 20, Valor = 540m,
                        DataInicio = new DateOnly(2026, 9, 14), Descricao = "Modelagem da informação da construção." },
            new Curso { Nome = "Gestão de Projetos Ágeis", Area = AreaCurso.Gestao, CargaHoraria = 24, Vagas = 35, Valor = 0m,
                        DataInicio = new DateOnly(2026, 10, 5), Descricao = "Scrum e Kanban na prática. Curso gratuito." }
        };

        var alunos = new[]
        {
            new Aluno { Nome = "Ana Souza",     Email = "ana@email.com",    Telefone = "(19) 99999-0001", DataNascimento = new DateOnly(1998, 3, 12) },
            new Aluno { Nome = "Bruno Lima",    Email = "bruno@email.com",  Telefone = "(19) 99999-0002", DataNascimento = new DateOnly(1990, 7, 25) },
            new Aluno { Nome = "Carla Mendes",  Email = "carla@email.com",  Telefone = "(19) 99999-0003" },
            new Aluno { Nome = "Diego Rocha",   Email = "diego@email.com" },
            new Aluno { Nome = "Elisa Martins", Email = "elisa@email.com",  DataNascimento = new DateOnly(2001, 11, 2) }
        };

        contexto.Cursos.AddRange(cursos);
        contexto.Alunos.AddRange(alunos);

        contexto.Matriculas.AddRange(
            new Matricula { Curso = cursos[0], Aluno = alunos[0] },
            new Matricula { Curso = cursos[0], Aluno = alunos[1] },
            new Matricula { Curso = cursos[1], Aluno = alunos[0], Status = StatusMatricula.Concluida, NotaFinal = 9.5m },
            new Matricula { Curso = cursos[2], Aluno = alunos[1] },
            new Matricula { Curso = cursos[2], Aluno = alunos[3] },
            new Matricula { Curso = cursos[2], Aluno = alunos[4] },
            new Matricula { Curso = cursos[3], Aluno = alunos[2], Status = StatusMatricula.Cancelada });

        contexto.SaveChanges();
    }
}
