using ControleFinanceiro.Domain.Entities;

namespace ControleFinanceiro.Domain.Ports;

public interface IResgatePontosRepository
{
    Task<List<ResgatePontos>> ListarUltimosAsync(int quantidade);
    Task<decimal> SomarTotalPontosResgatadosAsync();
    Task AdicionarAsync(ResgatePontos resgate);
}
