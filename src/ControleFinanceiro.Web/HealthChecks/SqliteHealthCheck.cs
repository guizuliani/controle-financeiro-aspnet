using ControleFinanceiro.Infrastructure.Persistence;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ControleFinanceiro.HealthChecks;

public class SqliteHealthCheck : IHealthCheck
{
    private readonly AppDbContext _dbContext;

    public SqliteHealthCheck(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            bool conectado = await _dbContext.Database.CanConnectAsync(cancellationToken);
            return conectado
                ? HealthCheckResult.Healthy("Banco de dados SQLite operacional.")
                : HealthCheckResult.Unhealthy("Não foi possível conectar ao banco SQLite.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Erro ao verificar banco SQLite.", ex);
        }
    }
}
