using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ControleFinanceiro.Data;
using ControleFinanceiro.Models;
using ControleFinanceiro.Models.Enums;
using ControleFinanceiro.Models.ViewModels;

namespace ControleFinanceiro.Controllers
{
    public class PontosController : Controller
    {
        private readonly AppDbContext _context;

        public PontosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Pontos
        public async Task<IActionResult> Index()
        {
            var cartoes = await _context.CartoesCredito
                .Include(c => c.Transacoes)
                .Include(c => c.Resgates)
                .Where(c => c.AcumulaPontos)
                .ToListAsync();

            var programas = cartoes.Select(c =>
            {
                var totalPontosGanhos = c.Transacoes.Sum(t => t.PontosGerados);
                var totalPontosResgatados = c.Resgates.Sum(r => r.Pontos);

                string regraTexto = c.TipoRegra switch
                {
                    TipoRegraPontos.PorDolar => $"{c.FatorPontuacao:N1} pts / US$ 1 (Dólar R$ {c.CotacaoDolarReferencia:N2})",
                    TipoRegraPontos.PorReal => $"{c.FatorPontuacao:N1} pts / R$ 1 gasto",
                    TipoRegraPontos.Cashback => $"{c.FatorPontuacao:N1}% de Cashback",
                    _ => "Configurado"
                };

                return new ProgramaPontosItem
                {
                    CartaoId = c.Id,
                    CartaoNome = c.Nome,
                    NomePrograma = c.NomeProgramaPontos ?? "Programa de Pontos",
                    CorHex = c.CorHex,
                    TotalAcumulado = totalPontosGanhos,
                    TotalResgatado = totalPontosResgatados,
                    RegraDescricao = regraTexto
                };
            }).ToList();

            var extratoTransacoes = await _context.Transacoes
                .Include(t => t.CartaoCredito)
                .Include(t => t.Categoria)
                .Where(t => t.PontosGerados > 0)
                .OrderByDescending(t => t.Data)
                .Take(25)
                .ToListAsync();

            var historicoResgates = await _context.ResgatesPontos
                .Include(r => r.CartaoCredito)
                .OrderByDescending(r => r.Data)
                .Take(25)
                .ToListAsync();

            var viewModel = new PontosViewModel
            {
                Programas = programas,
                ExtratoTransacoes = extratoTransacoes,
                HistoricoResgates = historicoResgates
            };

            return View(viewModel);
        }

        // GET: Pontos/Resgatar
        public async Task<IActionResult> Resgatar()
        {
            var cartoes = await _context.CartoesCredito.Where(c => c.AcumulaPontos).ToListAsync();
            ViewBag.Cartoes = new SelectList(cartoes, "Id", "Nome");
            return View(new ResgatePontos { Data = DateTime.Today });
        }

        // POST: Pontos/Resgatar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Resgatar([Bind("CartaoCreditoId,Data,Pontos,Descricao")] ResgatePontos resgate)
        {
            if (ModelState.IsValid)
            {
                var cartao = await _context.CartoesCredito
                    .Include(c => c.Transacoes)
                    .Include(c => c.Resgates)
                    .FirstOrDefaultAsync(c => c.Id == resgate.CartaoCreditoId);

                if (cartao != null)
                {
                    var saldoDisponivel = cartao.Transacoes.Sum(t => t.PontosGerados) - cartao.Resgates.Sum(r => r.Pontos);
                    if (resgate.Pontos > saldoDisponivel)
                    {
                        ModelState.AddModelError("Pontos", $"Saldo insuficiente! Saldo disponível neste cartão: {saldoDisponivel:N0} pontos.");
                        var cartoesDisponiveis = await _context.CartoesCredito.Where(c => c.AcumulaPontos).ToListAsync();
                        ViewBag.Cartoes = new SelectList(cartoesDisponiveis, "Id", "Nome", resgate.CartaoCreditoId);
                        return View(resgate);
                    }
                }

                _context.ResgatesPontos.Add(resgate);
                await _context.SaveChangesAsync();
                TempData["Sucesso"] = $"Resgate de {resgate.Pontos:N0} pontos registrado com sucesso!";
                return RedirectToAction(nameof(Index));
            }

            var cartoes = await _context.CartoesCredito.Where(c => c.AcumulaPontos).ToListAsync();
            ViewBag.Cartoes = new SelectList(cartoes, "Id", "Nome", resgate.CartaoCreditoId);
            return View(resgate);
        }
    }
}
