using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Application.Ports;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Ports;

namespace ControleFinanceiro.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly ITransacaoRepository _transacaoRepository;
    private readonly ICartaoCreditoRepository _cartaoRepository;
    private readonly IResgatePontosRepository _resgateRepository;
    private readonly IContaBancariaRepository _contaRepository;
    private readonly IOrcamentoRepository _orcamentoRepository;

    public DashboardService(
        ITransacaoRepository transacaoRepository,
        ICartaoCreditoRepository cartaoRepository,
        IResgatePontosRepository resgateRepository,
        IContaBancariaRepository contaRepository,
        IOrcamentoRepository orcamentoRepository)
    {
        _transacaoRepository = transacaoRepository;
        _cartaoRepository = cartaoRepository;
        _resgateRepository = resgateRepository;
        _contaRepository = contaRepository;
        _orcamentoRepository = orcamentoRepository;
    }

    public async Task<DashboardDto> ObterDashboardAsync(int mes, int ano, int usuarioId = 1)
    {
        var transacoesContaMes = await _transacaoRepository.ListarContaCorrentePorMesAsync(mes, ano, usuarioId);

        decimal totalReceitas = transacoesContaMes
            .Where(t => t.Tipo == TipoTransacao.Receita)
            .Sum(t => t.Valor);

        decimal totalDespesas = transacoesContaMes
            .Where(t => t.Tipo == TipoTransacao.Despesa)
            .Sum(t => t.Valor);

        var transacoesFaturaMes = await _transacaoRepository.ListarFaturaPorMesAsync(mes, ano, usuarioId: usuarioId);
        decimal totalFaturasMes = transacoesFaturaMes.Sum(t => t.Valor);

        var todasDespesasMes = await _transacaoRepository.ListarDespesasCompetenciaMesAsync(mes, ano, usuarioId);
        var despesasPorCategoria = todasDespesasMes
            .GroupBy(t => t.Categoria != null ? t.Categoria.Nome : "Sem Categoria")
            .Select(g => new GraficoCategoriaDto
            {
                Categoria = g.Key,
                Total = g.Sum(t => t.Valor),
                CorHex = g.First().Categoria?.CorHex ?? "#6c757d"
            })
            .OrderByDescending(g => g.Total)
            .ToList();

        var ultimasTransacoes = await _transacaoRepository.ListarUltimasAsync(7, usuarioId);

        var cartoesDb = await _cartaoRepository.ListarTodosOrdenadosPorNomeAsync(incluirTransacoes: true, usuarioId: usuarioId);
        var cartoesResumo = cartoesDb.Select(c => new CartaoResumoDto
        {
            Id = c.Id,
            Nome = c.Nome,
            CorHex = c.CorHex,
            LimiteTotal = c.LimiteTotal,
            LimiteUtilizado = c.CalcularLimiteUtilizado(),
            FaturaMesAtual = c.Transacoes
                .Where(t => t.FaturaMes == mes && t.FaturaAno == ano)
                .Sum(t => t.Valor),
            DiaFechamento = c.DiaFechamento,
            DiaVencimento = c.DiaVencimento,
            NomeProgramaPontos = c.AcumulaPontos ? (c.NomeProgramaPontos ?? "Pontos") : "Sem Pontos"
        }).ToList();

        var contasDb = await _contaRepository.ListarPorUsuarioAsync(usuarioId, incluirTransacoes: true);
        var contasResumo = contasDb.Select(c => new ContaBancariaResumoDto
        {
            Id = c.Id,
            Nome = c.Nome,
            Instituicao = c.Instituicao,
            Tipo = c.Tipo,
            SaldoInicial = c.SaldoInicial,
            SaldoAtual = c.CalcularSaldoAtual(),
            CorHex = c.CorHex
        }).ToList();

        var orcamentosDb = await _orcamentoRepository.ListarPorMesAsync(usuarioId, mes, ano);
        var gastosPorCatId = todasDespesasMes
            .GroupBy(t => t.CategoriaId)
            .ToDictionary(g => g.Key, g => g.Sum(t => t.Valor));

        var orcamentosResumo = orcamentosDb.Select(o =>
        {
            var gasto = gastosPorCatId.GetValueOrDefault(o.CategoriaId, 0m);
            return new OrcamentoResumoDto
            {
                Id = o.Id,
                CategoriaId = o.CategoriaId,
                CategoriaNome = o.Categoria?.Nome ?? "Categoria",
                CategoriaCorHex = o.Categoria?.CorHex ?? "#6c757d",
                ValorLimite = o.ValorLimite,
                ValorGasto = gasto,
                PercentualUso = o.CalcularPercentualUso(gasto),
                NivelAlerta = o.ObterNivelAlerta(gasto),
                Mes = o.Mes,
                Ano = o.Ano
            };
        }).OrderByDescending(x => x.PercentualUso).ToList();

        var totalPontosGanhos = await _transacaoRepository.SomarTotalPontosGeradosAsync(usuarioId);
        var totalPontosResgatados = await _resgateRepository.SomarTotalPontosResgatadosAsync(usuarioId);
        var saldoPontos = Math.Max(0, totalPontosGanhos - totalPontosResgatados);

        return new DashboardDto
        {
            TotalReceitas = totalReceitas,
            TotalDespesas = totalDespesas,
            TotalFaturasMes = totalFaturasMes,
            TotalPontosAcumulados = saldoPontos,
            MesAtual = mes,
            AnoAtual = ano,
            UltimasTransacoes = ultimasTransacoes,
            DespesasPorCategoria = despesasPorCategoria,
            CartoesResumo = cartoesResumo,
            ContasResumo = contasResumo,
            OrcamentosResumo = orcamentosResumo
        };
    }
}
