using ControleFinanceiro.Application.Ports;
using ControleFinanceiro.Application.Services;
using ControleFinanceiro.Domain.Ports;
using ControleFinanceiro.Infrastructure.Persistence;
using ControleFinanceiro.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ControleFinanceiro.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddHexagonalArchitecture(this IServiceCollection services, string? connectionString)
    {
        // Driven Adapter: Persistence (EF Core + SQLite)
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(connectionString));

        // Driven Ports -> Driven Adapters (Repositories & UnitOfWork)
        services.AddScoped<ICartaoCreditoRepository, CartaoCreditoRepository>();
        services.AddScoped<ICategoriaRepository, CategoriaRepository>();
        services.AddScoped<ITransacaoRepository, TransacaoRepository>();
        services.AddScoped<IResgatePontosRepository, ResgatePontosRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Driving Ports -> Application Use Cases
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<ITransacaoService, TransacaoService>();
        services.AddScoped<ICartaoService, CartaoService>();
        services.AddScoped<ICategoriaService, CategoriaService>();
        services.AddScoped<IPontosService, PontosService>();

        return services;
    }

    public static void GarantirBancoDadosCriado(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Database.EnsureCreated();
    }
}
