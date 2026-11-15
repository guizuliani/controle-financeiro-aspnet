using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ControleFinanceiro.Data;
using ControleFinanceiro.Models;
using ControleFinanceiro.Models.Enums;
using ControleFinanceiro.Models.ViewModels;

namespace ControleFinanceiro.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? mes, int? ano)
        {
            var dataAtual = DateTime.Today;
            int mesFiltro = mes ?? dataAtual.Month;
            int anoFiltro = ano ?? dataAtual.Year;

            // Transações diretas em conta corrente (Receitas e Despesas do mês)
            var transacoesContaMes = await _context.Transacoes
                .Include(t => t.Categoria)
                .Where(t => t.FormaPagamento == FormaPagamento.ContaCorrente && t.Data.Month == mesFiltro && t.Data.Year == anoFiltro)
                .ToListAsync();

            decimal totalReceitas = transacoesContaMes
                .Where(t => t.Tipo == TipoTransacao.Receita)
                .Sum(t => t.Valor);

            decimal totalDespesas = transacoesContaMes
                .Where(t => t.Tipo == TipoTransacao.Despesa)
                .Sum(t => t.Valor);

            // Faturas de cartão com vencimento no mês selecionado
            var transacoesCartaoFaturaMes = await _context.Transacoes
                .Include(t => t.CartaoCredito)
                .Where(t => t.FormaPagamento == FormaPagamento.CartaoCredito && t.FaturaMes == mesFiltro && t.FaturaAno == anoFiltro)
                .ToListAsync();

            decimal totalFaturasMes = transacoesCartaoFaturaMes.Sum(t => t.Valor);

            // Despesas por Categoria (todas as despesas do mês, incluindo cartão)
            var todasDespesasMes = await _context.Transacoes
                .Include(t => t.Categoria)
                .Where(t => t.Tipo == TipoTransacao.Despesa && 
                    ((t.FormaPagamento == FormaPagamento.ContaCorrente && t.Data.Month == mesFiltro && t.Data.Year == anoFiltro) ||
                     (t.FormaPagamento == FormaPagamento.CartaoCredito && t.FaturaMes == mesFiltro && t.FaturaAno == anoFiltro)))
                .ToListAsync();

            var despesasPorCategoria = todasDespesasMes
                .GroupBy(t => t.Categoria != null ? t.Categoria.Nome : "Sem Categoria")
                .Select(g => new GraficoCategoriaItem
                {
                    Categoria = g.Key,
                    Total = g.Sum(t => t.Valor),
                    CorHex = g.First().Categoria?.CorHex ?? "#6c757d"
                })
                .OrderByDescending(g => g.Total)
                .ToList();

            // Últimos lançamentos
            var ultimasTransacoes = await _context.Transacoes
                .Include(t => t.Categoria)
                .Include(t => t.CartaoCredito)
                .OrderByDescending(t => t.Data)
                .ThenByDescending(t => t.Id)
                .Take(7)
                .ToListAsync();

            // Resumo dos cartões cadastrados
            var cartoesDb = await _context.CartoesCredito
                .Include(c => c.Transacoes)
                .OrderBy(c => c.Nome)
                .ToListAsync();

            var cartoesResumo = cartoesDb.Select(c =>
            {
                var limiteUtilizado = c.Transacoes.Where(t => !t.FaturaPaga).Sum(t => t.Valor);
                var faturaMesAtual = c.Transacoes
                    .Where(t => t.FaturaMes == mesFiltro && t.FaturaAno == anoFiltro)
                    .Sum(t => t.Valor);

                return new CartaoResumoItem
                {
                    Id = c.Id,
                    Nome = c.Nome,
                    CorHex = c.CorHex,
                    LimiteTotal = c.LimiteTotal,
                    LimiteUtilizado = limiteUtilizado,
                    FaturaMesAtual = faturaMesAtual,
                    DiaFechamento = c.DiaFechamento,
                    DiaVencimento = c.DiaVencimento,
                    NomeProgramaPontos = c.AcumulaPontos ? (c.NomeProgramaPontos ?? "Pontos") : "Sem Pontos"
                };
            }).ToList();

            // Total de pontos acumulados líquido
            var totalPontosGanhos = await _context.Transacoes.SumAsync(t => t.PontosGerados);
            var totalPontosResgatados = await _context.ResgatesPontos.SumAsync(r => r.Pontos);
            var saldoPontos = Math.Max(0, totalPontosGanhos - totalPontosResgatados);

            var viewModel = new DashboardViewModel
            {
                TotalReceitas = totalReceitas,
                TotalDespesas = totalDespesas,
                TotalFaturasMes = totalFaturasMes,
                TotalPontosAcumulados = saldoPontos,
                MesAtual = mesFiltro,
                AnoAtual = anoFiltro,
                UltimasTransacoes = ultimasTransacoes,
                DespesasPorCategoria = despesasPorCategoria,
                CartoesResumo = cartoesResumo
            };

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
