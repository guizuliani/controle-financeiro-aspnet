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
        string? busca,
        int? contaBancariaId = null,
        int usuarioId = 1);

    Task<Transacao?> ObterPorIdAsync(int id, bool incluirRelacionamentos = false, int usuarioId = 1);
    Task<List<Categoria>> ListarCategoriasParaSelecaoAsync(int usuarioId = 1);
    Task<List<CartaoCredito>> ListarCartoesParaSelecaoAsync(int usuarioId = 1);
    Task<List<ContaBancaria>> ListarContasParaSelecaoAsync(int usuarioId = 1);
    Task<ResultadoOperacao> CriarAsync(Transacao transacao);
    Task<ResultadoOperacao> AtualizarAsync(Transacao transacao);
    Task<ResultadoOperacao> ExcluirAsync(int id, int usuarioId = 1);
}
