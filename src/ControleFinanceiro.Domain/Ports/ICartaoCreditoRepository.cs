using ControleFinanceiro.Domain.Entities;

namespace ControleFinanceiro.Domain.Ports;

public interface ICartaoCreditoRepository
{
    Task<List<CartaoCredito>> ListarTodosOrdenadosPorNomeAsync(bool incluirTransacoes = false, bool incluirResgates = false, int usuarioId = 1);
    Task<List<CartaoCredito>> ListarComPontosAsync(bool incluirTransacoesEResgates = false, int usuarioId = 1);
    Task<CartaoCredito?> ObterPorIdAsync(int id, bool incluirTransacoes = false, bool incluirResgates = false, int usuarioId = 1);
    Task<bool> ExisteAsync(int id, int usuarioId = 1);
    Task AdicionarAsync(CartaoCredito cartao);
    void Atualizar(CartaoCredito cartao);
    void Remover(CartaoCredito cartao);
}
