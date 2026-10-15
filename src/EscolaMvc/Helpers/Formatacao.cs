using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;

namespace EscolaMvc.Helpers;

/// <summary>Formatação no padrão brasileiro, usada nas views.</summary>
public static class Formatacao
{
    private static readonly CultureInfo PtBr = new("pt-BR");

    public static string Reais(decimal valor) => valor == 0 ? "Gratuito" : valor.ToString("C", PtBr);

    public static string Data(DateOnly data) => data.ToString("dd/MM/yyyy", PtBr);

    public static string Data(DateTime data) => data.ToString("dd/MM/yyyy", PtBr);

    public static string Data(DateOnly? data) => data is null ? "—" : Data(data.Value);

    /// <summary>Nome amigável de um enum, lido do atributo [Display(Name = ...)].</summary>
    public static string Exibir(Enum valor) =>
        valor.GetType().GetField(valor.ToString())?.GetCustomAttribute<DisplayAttribute>()?.GetName()
        ?? valor.ToString();

    public static string Nota(decimal? nota) => nota?.ToString("0.0", PtBr) ?? "—";
}
