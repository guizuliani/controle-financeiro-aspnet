using ControleFinanceiro.HealthChecks;
using ControleFinanceiro.Infrastructure;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Data.Sqlite;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Add services to the container (Driving & Driven Adapters).
builder.Services.AddControllersWithViews();
builder.Services.AddHexagonalArchitecture(connectionString);

// Persistir chaves de DataProtection junto à pasta de dados (evita perda de sessão no Docker)
var dbDir = !string.IsNullOrWhiteSpace(connectionString)
    ? Path.GetDirectoryName(new SqliteConnectionStringBuilder(connectionString).DataSource)
    : null;
var keysPath = string.IsNullOrWhiteSpace(dbDir)
    ? Path.Combine(builder.Environment.ContentRootPath, "App_Data", "keys")
    : Path.Combine(dbDir, "keys");
Directory.CreateDirectory(keysPath);

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(keysPath))
    .SetApplicationName("ControleFinanceiro");

// Autenticação baseada em Cookie
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
    });

// Health Checks (/health)
builder.Services.AddHealthChecks()
    .AddCheck<SqliteHealthCheck>("sqlite");

var app = builder.Build();

// Aplicar EF Core Migrations e garantir criação do banco de dados
DependencyInjection.GarantirBancoDadosCriado(app.Services);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health").AllowAnonymous();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
