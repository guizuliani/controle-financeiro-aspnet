using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ControleFinanceiro.Application.Ports;
using ControleFinanceiro.Models;
using ControleFinanceiro.Models.ViewModels;

namespace ControleFinanceiro.Controllers;

public class HomeController : Controller
{
    private readonly IDashboardService _dashboardService;

    public HomeController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    public async Task<IActionResult> Index(int? mes, int? ano)
    {
        var dataAtual = DateTime.Today;
        int mesFiltro = mes ?? dataAtual.Month;
        int anoFiltro = ano ?? dataAtual.Year;

        var dto = await _dashboardService.ObterDashboardAsync(mesFiltro, anoFiltro);

        var viewModel = new DashboardViewModel
        {
            TotalReceitas = dto.TotalReceitas,
            TotalDespesas = dto.TotalDespesas,
            TotalFaturasMes = dto.TotalFaturasMes,
            TotalPontosAcumulados = dto.TotalPontosAcumulados,
            MesAtual = dto.MesAtual,
            AnoAtual = dto.AnoAtual,
            UltimasTransacoes = dto.UltimasTransacoes,
            DespesasPorCategoria = dto.DespesasPorCategoria.Select(c => new GraficoCategoriaItem
            {
                Categoria = c.Categoria,
                Total = c.Total,
                CorHex = c.CorHex
            }).ToList(),
            CartoesResumo = dto.CartoesResumo.Select(c => new CartaoResumoItem
            {
                Id = c.Id,
                Nome = c.Nome,
                CorHex = c.CorHex,
                LimiteTotal = c.LimiteTotal,
                LimiteUtilizado = c.LimiteUtilizado,
                FaturaMesAtual = c.FaturaMesAtual,
                DiaFechamento = c.DiaFechamento,
                DiaVencimento = c.DiaVencimento,
                NomeProgramaPontos = c.NomeProgramaPontos
            }).ToList()
        };

        return View(viewModel);
    }

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
