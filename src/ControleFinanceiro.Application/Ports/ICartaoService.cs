using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Domain.Entities;

namespace ControleFinanceiro.Application.Ports;

public interface ICartaoService
{
    Task<List<CartaoResumoDto>> ListarResumosAsync(int mes, int ano);
    Task<CartaoCredito?> ObterPorIdAsync(int id, bool incluirTransacoes = false);
    Task<ResultadoOperacao> CriarAsync(CartaoCredito cartao);
    Task<ResultadoOperacao> AtualizarAsync(CartaoCredito cartao);
    Task<ResultadoOperacao> ExcluirAsync(int id);
    Task<FaturaDto?> ObterFaturaAsync(int cartaoId, int mes, int ano);
    Task<ResultadoOperacao> PagarFaturaAsync(int cartaoId, int mes, int ano);
}
