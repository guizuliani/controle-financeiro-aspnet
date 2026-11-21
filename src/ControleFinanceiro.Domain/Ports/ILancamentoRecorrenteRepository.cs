using ControleFinanceiro.Domain.Entities;

namespace ControleFinanceiro.Domain.Ports;

public interface ILancamentoRecorrenteRepository
{
    Task<List<LancamentoRecorrente>> ListarPorUsuarioAsync(int usuarioId);
    Task<List<LancamentoRecorrente>> ListarAtivosPorUsuarioAsync(int usuarioId);
    Task<LancamentoRecorrente?> ObterPorIdAsync(int id, int usuarioId);
    Task AdicionarAsync(LancamentoRecorrente recorrente);
    void Atualizar(LancamentoRecorrente recorrente);
    void Remover(LancamentoRecorrente recorrente);
}
