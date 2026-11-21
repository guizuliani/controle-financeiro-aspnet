using ControleFinanceiro.Domain.Entities;

namespace ControleFinanceiro.Domain.Ports;

public interface ICategoriaRepository
{
    Task<List<Categoria>> ListarTodasOrdenadasPorNomeAsync(bool incluirTransacoes = false, int usuarioId = 1);
    Task<Categoria?> ObterPorIdAsync(int id, bool incluirTransacoes = false, int usuarioId = 1);
    Task<Categoria?> ObterCategoriaPagamentoFaturaAsync(int usuarioId = 1);
    Task<bool> ExisteAsync(int id, int usuarioId = 1);
    Task AdicionarAsync(Categoria categoria);
    Task AdicionarVariasAsync(IEnumerable<Categoria> categorias);
    void Atualizar(Categoria categoria);
    void Remover(Categoria categoria);
}
