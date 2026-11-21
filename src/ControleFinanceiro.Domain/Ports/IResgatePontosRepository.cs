using ControleFinanceiro.Domain.Entities;

namespace ControleFinanceiro.Domain.Ports;

public interface IResgatePontosRepository
{
    Task<List<ResgatePontos>> ListarUltimosAsync(int quantidade, int usuarioId = 1);
    Task<decimal> SomarTotalPontosResgatadosAsync(int usuarioId = 1);
    Task AdicionarAsync(ResgatePontos resgate);
}
