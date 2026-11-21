using ControleFinanceiro.Domain.Entities;

namespace ControleFinanceiro.Domain.Ports;

public interface ICartaoCreditoRepository
{
    Task<List<CartaoCredito>> ListarTodosOrdenadosPorNomeAsync(bool incluirTransacoes = false, bool incluirResgates = false);
    Task<List<CartaoCredito>> ListarComPontosAsync(bool incluirTransacoesEResgates = false);
    Task<CartaoCredito?> ObterPorIdAsync(int id, bool incluirTransacoes = false, bool incluirResgates = false);
    Task<bool> ExisteAsync(int id);
    Task AdicionarAsync(CartaoCredito cartao);
    void Atualizar(CartaoCredito cartao);
    void Remover(CartaoCredito cartao);
}
