using acaigalatico.Infrastructure.Context; // <--- ADICIONE ISSO (1)
using Microsoft.EntityFrameworkCore;       // <--- ADICIONE ISSO (2)
using Microsoft.AspNetCore.Identity;       // <--- ADICIONE ISSO (3)
using acaigalatico.Domain.Interfaces;
using acaigalatico.Infrastructure.Repositories;
using acaigalatico.Application.Interfaces;
using acaigalatico.Application.Services;
using System.Globalization;
using Microsoft.AspNetCore.Localization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages(); // Adiciona suporte a Razor Pages (necessário para Identity)

// --- INICIO DA CONFIGURA��O DO BANCO ---

// 1. Pega a string de conex�o do appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 2. Configura o AppDbContext para usar SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString, b => b.MigrationsAssembly("acaigalatico.Infrastructure")));

// 3. Configura o Identity (Login)
// 3. Configura o Identity (Login)
builder.Services.AddDefaultIdentity<IdentityUser>(options => {
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false; // Aceita o '*' mesmo como false
    options.Password.RequireUppercase = false;

    // Altere para 8 para refletir sua nova senha padrão
    options.Password.RequiredLength = 8;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<AppDbContext>();
// ^ Essa parte b.MigrationsAssembly � essencial porque o Context est� em outro projeto!


// --- FIM DA CONFIGURA��O DO BANCO ---

// Registrar o servio de Seed
builder.Services.AddScoped<acaigalatico.Infrastructure.SeedingService>();

// Registrar Repositorios e Services
builder.Services.AddScoped<IProdutoRepository, ProdutoRepository>();
builder.Services.AddScoped<IProdutoService, ProdutoService>();
builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();
builder.Services.AddScoped<ICategoriaService, CategoriaService>();

var app = builder.Build();

// --- CONFIGURAÇÃO DE CULTURA (PT-BR) ---
var defaultCulture = new CultureInfo("pt-BR");
var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(defaultCulture),
    SupportedCultures = new List<CultureInfo> { defaultCulture },
    SupportedUICultures = new List<CultureInfo> { defaultCulture }
};
app.UseRequestLocalization(localizationOptions);
// ---------------------------------------

// --- APLICA MIGRAÇÕES AUTOMATICAMENTE ---
try
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<acaigalatico.Infrastructure.Context.AppDbContext>();
    db.Database.Migrate();
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "Falha ao aplicar migrações do banco.");
}
// ----------------------------------------

// --- BLOCO DE INICIALIZAÇÃO DE DADOS ---
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        try 
        {
            var seedingService = scope.ServiceProvider.GetRequiredService<acaigalatico.Infrastructure.SeedingService>();
            await seedingService.SeedAsync(); // Roda o código de povoar o banco
        }
        catch (Exception ex)
        {
            // Log do erro mas deixa a aplicação subir
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "Erro ao popular o banco de dados na inicialização.");
        }
    }
}
// ---------------------------------------


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
provider.Mappings[".avif"] = "image/avif";
app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = provider
});

app.UseRouting();

app.UseAuthentication(); // <--- IMPORTANTE: Adicionar antes de Authorization
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
