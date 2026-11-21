using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Domain.Entities;

namespace ControleFinanceiro.Application.Ports;

public interface ICartaoService
{
    Task<List<CartaoResumoDto>> ListarResumosAsync(int mes, int ano, int usuarioId = 1);
    Task<CartaoCredito?> ObterPorIdAsync(int id, bool incluirTransacoes = false, int usuarioId = 1);
    Task<ResultadoOperacao> CriarAsync(CartaoCredito cartao);
    Task<ResultadoOperacao> AtualizarAsync(CartaoCredito cartao);
    Task<ResultadoOperacao> ExcluirAsync(int id, int usuarioId = 1);
    Task<FaturaDto?> ObterFaturaAsync(int cartaoId, int mes, int ano, int usuarioId = 1);
    Task<ResultadoOperacao> PagarFaturaAsync(int cartaoId, int mes, int ano, int usuarioId = 1);
}
