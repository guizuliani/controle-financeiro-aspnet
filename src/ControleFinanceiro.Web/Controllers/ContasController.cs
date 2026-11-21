using ControleFinanceiro.Application.Ports;
using ControleFinanceiro.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace ControleFinanceiro.Controllers;

public class ContasController : BaseAuthController
{
    private readonly IContaBancariaService _contaService;

    public ContasController(IContaBancariaService contaService)
    {
        _contaService = contaService;
    }

    public async Task<IActionResult> Index()
    {
        var contas = await _contaService.ListarResumosAsync(UsuarioId);
        return View(contas);
    }

    public IActionResult Create() => View(new ContaBancaria());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Nome,Instituicao,Tipo,SaldoInicial,CorHex")] ContaBancaria conta)
    {
        if (!ModelState.IsValid)
            return View(conta);

        conta.UsuarioId = UsuarioId;
        var resultado = await _contaService.CriarAsync(conta);
        TempData["Sucesso"] = resultado.Mensagem;
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var conta = await _contaService.ObterPorIdAsync(id.Value, UsuarioId);
        if (conta == null) return NotFound();
        return View(conta);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Nome,Instituicao,Tipo,SaldoInicial,CorHex")] ContaBancaria conta)
    {
        if (id != conta.Id) return NotFound();
        if (!ModelState.IsValid) return View(conta);

        conta.UsuarioId = UsuarioId;
        var resultado = await _contaService.AtualizarAsync(conta);
        TempData["Sucesso"] = resultado.Mensagem;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var resultado = await _contaService.ExcluirAsync(id, UsuarioId);
        TempData[resultado.Sucesso ? "Sucesso" : "Erro"] = resultado.Mensagem;
        return RedirectToAction(nameof(Index));
    }
}
