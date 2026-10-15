using System.Globalization;
using EscolaMvc.Data;
using EscolaMvc.Services;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// MVC + Entity Framework Core com SQLite (arquivo escola.db)
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<EscolaContext>(opcoes =>
    opcoes.UseSqlite(builder.Configuration.GetConnectionString("Escola")));
builder.Services.AddScoped<MatriculaService>();

var app = builder.Build();

// Cria o banco e os dados de exemplo na primeira execução.
// Em produção, prefira Migrations: dotnet ef migrations add Inicial && dotnet ef database update
using (var escopo = app.Services.CreateScope())
{
    var contexto = escopo.ServiceProvider.GetRequiredService<EscolaContext>();
    contexto.Database.EnsureCreated();
    DadosIniciais.Popular(contexto);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Os formulários são lidos com a cultura invariante: o <input type="number"> do HTML
// sempre envia o ponto como separador decimal ("390.50"), em qualquer navegador.
// A exibição em reais e datas brasileiras fica a cargo de Helpers/Formatacao.cs.
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(CultureInfo.InvariantCulture),
    SupportedCultures = [CultureInfo.InvariantCulture],
    SupportedUICultures = [CultureInfo.InvariantCulture]
});

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

// Rota convencional: /Controller/Action/Id
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
