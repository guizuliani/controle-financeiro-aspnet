using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Domain.Entities;

namespace ControleFinanceiro.Application.Ports;

public interface IOrcamentoService
{
    Task<List<OrcamentoResumoDto>> ListarResumosMesAsync(int mes, int ano, int usuarioId = 1);
    Task<OrcamentoCategoria?> ObterPorIdAsync(int id, int usuarioId = 1);
    Task<ResultadoOperacao> SalvarAsync(OrcamentoCategoria orcamento);
    Task<ResultadoOperacao> ExcluirAsync(int id, int usuarioId = 1);
}
