using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Domain.Entities;

namespace ControleFinanceiro.Application.Ports;

public interface IContaBancariaService
{
    Task<List<ContaBancariaResumoDto>> ListarResumosAsync(int usuarioId = 1);
    Task<ContaBancaria?> ObterPorIdAsync(int id, int usuarioId = 1);
    Task<ResultadoOperacao> CriarAsync(ContaBancaria conta);
    Task<ResultadoOperacao> AtualizarAsync(ContaBancaria conta);
    Task<ResultadoOperacao> ExcluirAsync(int id, int usuarioId = 1);
}
