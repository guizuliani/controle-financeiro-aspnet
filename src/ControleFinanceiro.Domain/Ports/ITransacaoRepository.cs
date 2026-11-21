using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Enums;

namespace ControleFinanceiro.Domain.Ports;

public interface ITransacaoRepository
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

    Task<List<Transacao>> ListarContaCorrentePorMesAsync(int mes, int ano, int usuarioId = 1);
    Task<List<Transacao>> ListarFaturaPorMesAsync(int mes, int ano, int? cartaoId = null, bool apenasNaoPagas = false, int usuarioId = 1);
    Task<List<Transacao>> ListarDespesasCompetenciaMesAsync(int mes, int ano, int usuarioId = 1);
    Task<List<Transacao>> ListarUltimasAsync(int quantidade, int usuarioId = 1);
    Task<List<Transacao>> ListarComPontosAsync(int quantidade, int usuarioId = 1);
    Task<decimal> SomarTotalPontosGeradosAsync(int usuarioId = 1);
    Task<Transacao?> ObterPorIdAsync(int id, bool incluirRelacionamentos = false, int usuarioId = 1);
    Task<bool> ExisteAsync(int id, int usuarioId = 1);
    Task AdicionarAsync(Transacao transacao);
    Task AdicionarVariasAsync(IEnumerable<Transacao> transacoes);
    void Atualizar(Transacao transacao);
    void Remover(Transacao transacao);
}
