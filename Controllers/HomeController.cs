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

            var transacoesMes = await _context.Transacoes
                .Include(t => t.Categoria)
                .Where(t => t.Data.Month == mesFiltro && t.Data.Year == anoFiltro)
                .ToListAsync();

            decimal totalReceitas = transacoesMes
                .Where(t => t.Tipo == TipoTransacao.Receita)
                .Sum(t => t.Valor);

            decimal totalDespesas = transacoesMes
                .Where(t => t.Tipo == TipoTransacao.Despesa)
                .Sum(t => t.Valor);

            var despesasPorCategoria = transacoesMes
                .Where(t => t.Tipo == TipoTransacao.Despesa)
                .GroupBy(t => t.Categoria != null ? t.Categoria.Nome : "Sem Categoria")
                .Select(g => new GraficoCategoriaItem
                {
                    Categoria = g.Key,
                    Total = g.Sum(t => t.Valor),
                    CorHex = g.First().Categoria?.CorHex ?? "#6c757d"
                })
                .OrderByDescending(g => g.Total)
                .ToList();

            var ultimasTransacoes = await _context.Transacoes
                .Include(t => t.Categoria)
                .OrderByDescending(t => t.Data)
                .ThenByDescending(t => t.Id)
                .Take(7)
                .ToListAsync();

            var viewModel = new DashboardViewModel
            {
                TotalReceitas = totalReceitas,
                TotalDespesas = totalDespesas,
                MesAtual = mesFiltro,
                AnoAtual = anoFiltro,
                UltimasTransacoes = ultimasTransacoes,
                DespesasPorCategoria = despesasPorCategoria
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
