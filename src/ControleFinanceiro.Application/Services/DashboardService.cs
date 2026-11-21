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

    public DashboardService(
        ITransacaoRepository transacaoRepository,
        ICartaoCreditoRepository cartaoRepository,
        IResgatePontosRepository resgateRepository)
    {
        _transacaoRepository = transacaoRepository;
        _cartaoRepository = cartaoRepository;
        _resgateRepository = resgateRepository;
    }

    public async Task<DashboardDto> ObterDashboardAsync(int mes, int ano)
    {
        var transacoesContaMes = await _transacaoRepository.ListarContaCorrentePorMesAsync(mes, ano);

        decimal totalReceitas = transacoesContaMes
            .Where(t => t.Tipo == TipoTransacao.Receita)
            .Sum(t => t.Valor);

        decimal totalDespesas = transacoesContaMes
            .Where(t => t.Tipo == TipoTransacao.Despesa)
            .Sum(t => t.Valor);

        var transacoesFaturaMes = await _transacaoRepository.ListarFaturaPorMesAsync(mes, ano);
        decimal totalFaturasMes = transacoesFaturaMes.Sum(t => t.Valor);

        var todasDespesasMes = await _transacaoRepository.ListarDespesasCompetenciaMesAsync(mes, ano);
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

        var ultimasTransacoes = await _transacaoRepository.ListarUltimasAsync(7);

        var cartoesDb = await _cartaoRepository.ListarTodosOrdenadosPorNomeAsync(incluirTransacoes: true);
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

        var totalPontosGanhos = await _transacaoRepository.SomarTotalPontosGeradosAsync();
        var totalPontosResgatados = await _resgateRepository.SomarTotalPontosResgatadosAsync();
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
            CartoesResumo = cartoesResumo
        };
    }
}
