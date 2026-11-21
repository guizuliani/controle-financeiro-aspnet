using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Domain.Entities;

namespace ControleFinanceiro.Application.Ports;

public interface ILancamentoRecorrenteService
{
    Task<List<LancamentoRecorrente>> ListarPorUsuarioAsync(int usuarioId = 1);
    Task<LancamentoRecorrente?> ObterPorIdAsync(int id, int usuarioId = 1);
    Task<ResultadoOperacao> CriarAsync(LancamentoRecorrente recorrente);
    Task<ResultadoOperacao> AlternarStatusAsync(int id, int usuarioId = 1);
    Task<ResultadoOperacao> ExcluirAsync(int id, int usuarioId = 1);
    Task<ResultadoOperacao> GerarLancamentosDoMesAsync(int mes, int ano, int usuarioId = 1);
}
