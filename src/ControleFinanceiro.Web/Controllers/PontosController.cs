using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ControleFinanceiro.Application.Ports;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Models.ViewModels;

namespace ControleFinanceiro.Controllers;

public class PontosController : BaseAuthController
{
    private readonly IPontosService _pontosService;

    public PontosController(IPontosService pontosService)
    {
        _pontosService = pontosService;
    }

    // GET: Pontos
    public async Task<IActionResult> Index()
    {
        var painel = await _pontosService.ObterPainelPontosAsync(UsuarioId);

        var viewModel = new PontosViewModel
        {
            Programas = painel.Programas.Select(p => new ProgramaPontosItem
            {
                CartaoId = p.CartaoId,
                CartaoNome = p.CartaoNome,
                NomePrograma = p.NomePrograma,
                CorHex = p.CorHex,
                TotalAcumulado = p.TotalAcumulado,
                TotalResgatado = p.TotalResgatado,
                RegraDescricao = p.RegraDescricao
            }).ToList(),
            ExtratoTransacoes = painel.ExtratoTransacoes,
            HistoricoResgates = painel.HistoricoResgates
        };

        return View(viewModel);
    }

    // GET: Pontos/Resgatar
    public async Task<IActionResult> Resgatar()
    {
        var cartoes = await _pontosService.ListarCartoesComPontosAsync(UsuarioId);
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
            resgate.UsuarioId = UsuarioId;
            var resultado = await _pontosService.ResgatarPontosAsync(resgate);
            if (!resultado.Sucesso)
            {
                ModelState.AddModelError(resultado.CampoErro ?? string.Empty, resultado.Mensagem);
                var cartoesDisponiveis = await _pontosService.ListarCartoesComPontosAsync(UsuarioId);
                ViewBag.Cartoes = new SelectList(cartoesDisponiveis, "Id", "Nome", resgate.CartaoCreditoId);
                return View(resgate);
            }

            TempData["Sucesso"] = resultado.Mensagem;
            return RedirectToAction(nameof(Index));
        }

        var cartoes = await _pontosService.ListarCartoesComPontosAsync(UsuarioId);
        ViewBag.Cartoes = new SelectList(cartoes, "Id", "Nome", resgate.CartaoCreditoId);
        return View(resgate);
    }
}
