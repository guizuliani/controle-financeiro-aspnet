using ControleFinanceiro.Domain.Entities;

namespace ControleFinanceiro.Domain.Ports;

public interface IOrcamentoRepository
{
    Task<List<OrcamentoCategoria>> ListarPorMesAsync(int usuarioId, int mes, int ano);
    Task<OrcamentoCategoria?> ObterPorIdAsync(int id, int usuarioId);
    Task<OrcamentoCategoria?> ObterPorCategoriaEMesAsync(int usuarioId, int categoriaId, int mes, int ano);
    Task AdicionarAsync(OrcamentoCategoria orcamento);
    void Atualizar(OrcamentoCategoria orcamento);
    void Remover(OrcamentoCategoria orcamento);
}
