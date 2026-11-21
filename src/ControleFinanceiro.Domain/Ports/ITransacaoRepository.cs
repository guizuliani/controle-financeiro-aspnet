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
        string? busca);

    Task<List<Transacao>> ListarContaCorrentePorMesAsync(int mes, int ano);
    Task<List<Transacao>> ListarFaturaPorMesAsync(int mes, int ano, int? cartaoId = null, bool apenasNaoPagas = false);
    Task<List<Transacao>> ListarDespesasCompetenciaMesAsync(int mes, int ano);
    Task<List<Transacao>> ListarUltimasAsync(int quantidade);
    Task<List<Transacao>> ListarComPontosAsync(int quantidade);
    Task<decimal> SomarTotalPontosGeradosAsync();
    Task<Transacao?> ObterPorIdAsync(int id, bool incluirRelacionamentos = false);
    Task<bool> ExisteAsync(int id);
    Task AdicionarAsync(Transacao transacao);
    Task AdicionarVariasAsync(IEnumerable<Transacao> transacoes);
    void Atualizar(Transacao transacao);
    void Remover(Transacao transacao);
}
