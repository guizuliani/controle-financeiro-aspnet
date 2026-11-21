using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Enums;

namespace ControleFinanceiro.Application.Ports;

public interface ITransacaoService
{
    Task<List<Transacao>> ListarFiltradasAsync(
        int? mes,
        int? ano,
        TipoTransacao? tipo,
        int? categoriaId,
        int? cartaoId,
        FormaPagamento? formaPagamento,
        string? busca);

    Task<Transacao?> ObterPorIdAsync(int id, bool incluirRelacionamentos = false);
    Task<List<Categoria>> ListarCategoriasParaSelecaoAsync();
    Task<List<CartaoCredito>> ListarCartoesParaSelecaoAsync();
    Task<ResultadoOperacao> CriarAsync(Transacao transacao);
    Task<ResultadoOperacao> AtualizarAsync(Transacao transacao);
    Task<ResultadoOperacao> ExcluirAsync(int id);
}
