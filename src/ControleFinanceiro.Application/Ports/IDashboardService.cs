using ControleFinanceiro.Application.DTOs;

namespace ControleFinanceiro.Application.Ports;

public interface IDashboardService
{
    Task<DashboardDto> ObterDashboardAsync(int mes, int ano);
}
