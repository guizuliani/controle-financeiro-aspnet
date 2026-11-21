using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Ports;
using ControleFinanceiro.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ControleFinanceiro.Infrastructure.Repositories;

public class TransacaoRepository : ITransacaoRepository
{
    private readonly AppDbContext _context;

    public TransacaoRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<List<Transacao>> ListarFiltradasAsync(
        int? mes,
        int? ano,
        TipoTransacao? tipo,
        int? categoriaId,
        int? cartaoId,
        FormaPagamento? formaPagamento,
        string? busca)
    {
        var query = _context.Transacoes
            .Include(t => t.Categoria)
            .Include(t => t.CartaoCredito)
            .AsQueryable();

        if (mes.HasValue)
            query = query.Where(t => t.Data.Month == mes.Value);

        if (ano.HasValue)
            query = query.Where(t => t.Data.Year == ano.Value);

        if (tipo.HasValue)
            query = query.Where(t => t.Tipo == tipo.Value);

        if (categoriaId.HasValue)
            query = query.Where(t => t.CategoriaId == categoriaId.Value);

        if (cartaoId.HasValue)
            query = query.Where(t => t.CartaoCreditoId == cartaoId.Value);

        if (formaPagamento.HasValue)
            query = query.Where(t => t.FormaPagamento == formaPagamento.Value);

        if (!string.IsNullOrWhiteSpace(busca))
            query = query.Where(t => t.Descricao.Contains(busca) || (t.Observacao != null && t.Observacao.Contains(busca)));

        return query
            .OrderByDescending(t => t.Data)
            .ThenByDescending(t => t.Id)
            .ToListAsync();
    }

    public Task<List<Transacao>> ListarContaCorrentePorMesAsync(int mes, int ano) =>
        _context.Transacoes
            .Include(t => t.Categoria)
            .Where(t => t.FormaPagamento == FormaPagamento.ContaCorrente && t.Data.Month == mes && t.Data.Year == ano)
            .ToListAsync();

    public Task<List<Transacao>> ListarFaturaPorMesAsync(
        int mes,
        int ano,
        int? cartaoId = null,
        bool apenasNaoPagas = false)
    {
        var query = _context.Transacoes
            .Include(t => t.Categoria)
            .Include(t => t.CartaoCredito)
            .Where(t => t.FormaPagamento == FormaPagamento.CartaoCredito && t.FaturaMes == mes && t.FaturaAno == ano);

        if (cartaoId.HasValue)
            query = query.Where(t => t.CartaoCreditoId == cartaoId.Value);

        if (apenasNaoPagas)
            query = query.Where(t => !t.FaturaPaga);

        return query.OrderBy(t => t.Data).ToListAsync();
    }

    public Task<List<Transacao>> ListarDespesasCompetenciaMesAsync(int mes, int ano) =>
        _context.Transacoes
            .Include(t => t.Categoria)
            .Where(t => t.Tipo == TipoTransacao.Despesa &&
                ((t.FormaPagamento == FormaPagamento.ContaCorrente && t.Data.Month == mes && t.Data.Year == ano) ||
                 (t.FormaPagamento == FormaPagamento.CartaoCredito && t.FaturaMes == mes && t.FaturaAno == ano)))
            .ToListAsync();

    public Task<List<Transacao>> ListarUltimasAsync(int quantidade) =>
        _context.Transacoes
            .Include(t => t.Categoria)
            .Include(t => t.CartaoCredito)
            .OrderByDescending(t => t.Data)
            .ThenByDescending(t => t.Id)
            .Take(quantidade)
            .ToListAsync();

    public Task<List<Transacao>> ListarComPontosAsync(int quantidade) =>
        _context.Transacoes
            .Include(t => t.CartaoCredito)
            .Include(t => t.Categoria)
            .Where(t => t.PontosGerados > 0)
            .OrderByDescending(t => t.Data)
            .Take(quantidade)
            .ToListAsync();

    public Task<decimal> SomarTotalPontosGeradosAsync() =>
        _context.Transacoes.SumAsync(t => t.PontosGerados);

    public Task<Transacao?> ObterPorIdAsync(int id, bool incluirRelacionamentos = false)
    {
        var query = _context.Transacoes.AsQueryable();

        if (incluirRelacionamentos)
        {
            query = query
                .Include(t => t.Categoria)
                .Include(t => t.CartaoCredito);
        }

        return query.FirstOrDefaultAsync(t => t.Id == id);
    }

    public Task<bool> ExisteAsync(int id) =>
        _context.Transacoes.AnyAsync(t => t.Id == id);

    public async Task AdicionarAsync(Transacao transacao) =>
        await _context.Transacoes.AddAsync(transacao);

    public async Task AdicionarVariasAsync(IEnumerable<Transacao> transacoes) =>
        await _context.Transacoes.AddRangeAsync(transacoes);

    public void Atualizar(Transacao transacao) =>
        _context.Transacoes.Update(transacao);

    public void Remover(Transacao transacao) =>
        _context.Transacoes.Remove(transacao);
}
