using ControleFinanceiro.Application.Ports;
using ControleFinanceiro.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ControleFinanceiro.Controllers;

public class RecorrenciasController : BaseAuthController
{
    private readonly ILancamentoRecorrenteService _recorrenteService;
    private readonly ITransacaoService _transacaoService;

    public RecorrenciasController(
        ILancamentoRecorrenteService recorrenteService,
        ITransacaoService transacaoService)
    {
        _recorrenteService = recorrenteService;
        _transacaoService = transacaoService;
    }

    public async Task<IActionResult> Index()
    {
        var lista = await _recorrenteService.ListarPorUsuarioAsync(UsuarioId);
        ViewBag.MesAtual = DateTime.Today.Month;
        ViewBag.AnoAtual = DateTime.Today.Year;
        return View(lista);
    }

    public async Task<IActionResult> Create()
    {
        await CarregarSelectLists();
        return View(new LancamentoRecorrente { DiaDoMes = 5 });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Descricao,Valor,Tipo,CategoriaId,FormaPagamento,ContaBancariaId,CartaoCreditoId,DiaDoMes")] LancamentoRecorrente model)
    {
        if (ModelState.IsValid)
        {
            model.UsuarioId = UsuarioId;
            var resultado = await _recorrenteService.CriarAsync(model);
            if (resultado.Sucesso)
            {
                TempData["Sucesso"] = resultado.Mensagem;
                return RedirectToAction(nameof(Index));
            }
            ModelState.AddModelError(resultado.CampoErro ?? string.Empty, resultado.Mensagem);
        }

        await CarregarSelectLists();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GerarMes(int mes, int ano)
    {
        var resultado = await _recorrenteService.GerarLancamentosDoMesAsync(mes, ano, UsuarioId);
        TempData[resultado.Sucesso ? "Sucesso" : "Erro"] = resultado.Mensagem;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AlternarStatus(int id)
    {
        var resultado = await _recorrenteService.AlternarStatusAsync(id, UsuarioId);
        TempData[resultado.Sucesso ? "Sucesso" : "Erro"] = resultado.Mensagem;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Excluir(int id)
    {
        var resultado = await _recorrenteService.ExcluirAsync(id, UsuarioId);
        TempData[resultado.Sucesso ? "Sucesso" : "Erro"] = resultado.Mensagem;
        return RedirectToAction(nameof(Index));
    }

    private async Task CarregarSelectLists()
    {
        ViewBag.Categorias = new SelectList(await _transacaoService.ListarCategoriasParaSelecaoAsync(UsuarioId), "Id", "Nome");
        ViewBag.Contas = new SelectList(await _transacaoService.ListarContasParaSelecaoAsync(UsuarioId), "Id", "Nome");
        ViewBag.Cartoes = new SelectList(await _transacaoService.ListarCartoesParaSelecaoAsync(UsuarioId), "Id", "Nome");
    }
}
