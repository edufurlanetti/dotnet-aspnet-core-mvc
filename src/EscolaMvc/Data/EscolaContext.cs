using EscolaMvc.Models;
using Microsoft.EntityFrameworkCore;

namespace EscolaMvc.Data;

/// <summary>
/// Contexto do Entity Framework Core: representa a sessão com o banco
/// e expõe uma coleção (DbSet) para cada tabela.
/// </summary>
public class EscolaContext(DbContextOptions<EscolaContext> options) : DbContext(options)
{
    public DbSet<Curso> Cursos => Set<Curso>();
    public DbSet<Aluno> Alunos => Set<Aluno>();
    public DbSet<Matricula> Matriculas => Set<Matricula>();

    // Fluent API: regras que não cabem em atributos
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Aluno>()
            .HasIndex(a => a.Email)
            .IsUnique();

        modelBuilder.Entity<Curso>()
            .Property(c => c.Area)
            .HasConversion<string>()          // grava o nome do enum, e não o número
            .HasMaxLength(20);

        modelBuilder.Entity<Matricula>()
            .Property(m => m.Status)
            .HasConversion<string>()
            .HasMaxLength(15);

        modelBuilder.Entity<Matricula>()
            .HasOne(m => m.Curso)
            .WithMany(c => c.Matriculas)
            .HasForeignKey(m => m.CursoId)
            .OnDelete(DeleteBehavior.Restrict);   // não apaga curso com matrículas

        modelBuilder.Entity<Matricula>()
            .HasOne(m => m.Aluno)
            .WithMany(a => a.Matriculas)
            .HasForeignKey(m => m.AlunoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
