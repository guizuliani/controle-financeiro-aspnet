using ControleFinanceiro.Domain.Entities;

namespace ControleFinanceiro.Domain.Ports;

public interface ICategoriaRepository
{
    Task<List<Categoria>> ListarTodasOrdenadasPorNomeAsync(bool incluirTransacoes = false);
    Task<Categoria?> ObterPorIdAsync(int id, bool incluirTransacoes = false);
    Task<Categoria?> ObterCategoriaPagamentoFaturaAsync();
    Task<bool> ExisteAsync(int id);
    Task AdicionarAsync(Categoria categoria);
    void Atualizar(Categoria categoria);
    void Remover(Categoria categoria);
}
