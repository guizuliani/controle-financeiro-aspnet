using Microsoft.AspNetCore.Mvc;
using ControleFinanceiro.Application.Ports;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Models.ViewModels;

namespace ControleFinanceiro.Controllers;

public class CartoesController : BaseAuthController
{
    private readonly ICartaoService _cartaoService;

    public CartoesController(ICartaoService cartaoService)
    {
        _cartaoService = cartaoService;
    }

    // GET: Cartoes
    public async Task<IActionResult> Index()
    {
        var dataAtual = DateTime.Today;
        var resumos = await _cartaoService.ListarResumosAsync(dataAtual.Month, dataAtual.Year, UsuarioId);

        var viewModelList = resumos.Select(c => new CartaoResumoItem
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
        }).ToList();

        return View(viewModelList);
    }

    // GET: Cartoes/Create
    public IActionResult Create() => View(new CartaoCredito());

    // POST: Cartoes/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Nome,Bandeira,CorHex,LimiteTotal,DiaFechamento,DiaVencimento,AcumulaPontos,NomeProgramaPontos,TipoRegra,FatorPontuacao,CotacaoDolarReferencia")] CartaoCredito cartao)
    {
        if (ModelState.IsValid)
        {
            cartao.UsuarioId = UsuarioId;
            var resultado = await _cartaoService.CriarAsync(cartao);
            TempData["Sucesso"] = resultado.Mensagem;
            return RedirectToAction(nameof(Index));
        }
        return View(cartao);
    }

    // GET: Cartoes/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var cartao = await _cartaoService.ObterPorIdAsync(id.Value, usuarioId: UsuarioId);
        if (cartao == null) return NotFound();

        return View(cartao);
    }

    // POST: Cartoes/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Nome,Bandeira,CorHex,LimiteTotal,DiaFechamento,DiaVencimento,AcumulaPontos,NomeProgramaPontos,TipoRegra,FatorPontuacao,CotacaoDolarReferencia")] CartaoCredito cartao)
    {
        if (id != cartao.Id) return NotFound();

        if (ModelState.IsValid)
        {
            cartao.UsuarioId = UsuarioId;
            var resultado = await _cartaoService.AtualizarAsync(cartao);
            TempData["Sucesso"] = resultado.Mensagem;
            return RedirectToAction(nameof(Index));
        }
        return View(cartao);
    }

    // GET: Cartoes/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var cartao = await _cartaoService.ObterPorIdAsync(id.Value, incluirTransacoes: true, usuarioId: UsuarioId);
        if (cartao == null) return NotFound();

        return View(cartao);
    }

    // POST: Cartoes/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var resultado = await _cartaoService.ExcluirAsync(id, UsuarioId);
        TempData[resultado.Sucesso ? "Sucesso" : "Erro"] = resultado.Mensagem;
        return RedirectToAction(nameof(Index));
    }

    // GET: Cartoes/Fatura/5
    public async Task<IActionResult> Fatura(int id, int? mes, int? ano)
    {
        var dataAtual = DateTime.Today;
        int mesFiltro = mes ?? dataAtual.Month;
        int anoFiltro = ano ?? dataAtual.Year;

        var fatura = await _cartaoService.ObterFaturaAsync(id, mesFiltro, anoFiltro, UsuarioId);
        if (fatura == null) return NotFound();

        var viewModel = new FaturaViewModel
        {
            Cartao = fatura.Cartao,
            Mes = fatura.Mes,
            Ano = fatura.Ano,
            Transacoes = fatura.Transacoes
        };

        return View(viewModel);
    }

    // POST: Cartoes/PagarFatura
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PagarFatura(int id, int mes, int ano)
    {
        var resultado = await _cartaoService.PagarFaturaAsync(id, mes, ano, UsuarioId);
        if (!resultado.Sucesso && resultado.Mensagem == "Cartão não encontrado.")
            return NotFound();

        TempData[resultado.Sucesso ? "Sucesso" : "Erro"] = resultado.Mensagem;
        return RedirectToAction(nameof(Fatura), new { id, mes, ano });
    }
}
