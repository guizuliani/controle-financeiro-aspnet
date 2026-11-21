using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ControleFinanceiro.Application.Ports;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Enums;

namespace ControleFinanceiro.Controllers;

public class TransacoesController : BaseAuthController
{
    private readonly ITransacaoService _transacaoService;

    public TransacoesController(ITransacaoService transacaoService)
    {
        _transacaoService = transacaoService;
    }

    // GET: Transacoes
    public async Task<IActionResult> Index(int? mes, int? ano, TipoTransacao? tipo, int? categoriaId, int? cartaoId, int? contaBancariaId, FormaPagamento? formaPagamento, string? busca)
    {
        var dataAtual = DateTime.Today;
        int mesFiltro = mes ?? dataAtual.Month;
        int anoFiltro = ano ?? dataAtual.Year;

        var transacoes = await _transacaoService.ListarFiltradasAsync(
            mes, ano, tipo, categoriaId, cartaoId, formaPagamento, busca, contaBancariaId, UsuarioId);

        ViewBag.Mes = mesFiltro;
        ViewBag.Ano = anoFiltro;
        ViewBag.Tipo = tipo;
        ViewBag.CategoriaId = categoriaId;
        ViewBag.CartaoId = cartaoId;
        ViewBag.ContaBancariaId = contaBancariaId;
        ViewBag.FormaPagamento = formaPagamento;
        ViewBag.Busca = busca;

        ViewBag.Categorias = new SelectList(await _transacaoService.ListarCategoriasParaSelecaoAsync(UsuarioId), "Id", "Nome", categoriaId);
        ViewBag.Cartoes = new SelectList(await _transacaoService.ListarCartoesParaSelecaoAsync(UsuarioId), "Id", "Nome", cartaoId);
        ViewBag.Contas = new SelectList(await _transacaoService.ListarContasParaSelecaoAsync(UsuarioId), "Id", "Nome", contaBancariaId);

        return View(transacoes);
    }

    // GET: Transacoes/Create
    public async Task<IActionResult> Create()
    {
        await CarregarSelectLists();
        return View(new Transacao { Data = DateTime.Today, TotalParcelas = 1 });
    }

    // POST: Transacoes/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Descricao,Valor,Data,Tipo,CategoriaId,Observacao,FormaPagamento,ContaBancariaId,CartaoCreditoId,TotalParcelas")] Transacao transacao)
    {
        if (transacao.FormaPagamento == FormaPagamento.CartaoCredito && (!transacao.CartaoCreditoId.HasValue || transacao.CartaoCreditoId == 0))
        {
            ModelState.AddModelError("CartaoCreditoId", "Selecione o cartão de crédito para esta forma de pagamento.");
        }

        if (ModelState.IsValid)
        {
            transacao.UsuarioId = UsuarioId;
            var resultado = await _transacaoService.CriarAsync(transacao);
            if (!resultado.Sucesso)
            {
                ModelState.AddModelError(resultado.CampoErro ?? string.Empty, resultado.Mensagem);
                await CarregarSelectLists(transacao.CategoriaId, transacao.CartaoCreditoId, transacao.ContaBancariaId);
                return View(transacao);
            }

            TempData["Sucesso"] = resultado.Mensagem;
            return RedirectToAction(nameof(Index));
        }

        await CarregarSelectLists(transacao.CategoriaId, transacao.CartaoCreditoId, transacao.ContaBancariaId);
        return View(transacao);
    }

    // GET: Transacoes/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var transacao = await _transacaoService.ObterPorIdAsync(id.Value, usuarioId: UsuarioId);
        if (transacao == null) return NotFound();

        await CarregarSelectLists(transacao.CategoriaId, transacao.CartaoCreditoId, transacao.ContaBancariaId);
        return View(transacao);
    }

    // POST: Transacoes/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Descricao,Valor,Data,Tipo,CategoriaId,Observacao,FormaPagamento,ContaBancariaId,CartaoCreditoId,NumeroParcela,TotalParcelas,FaturaMes,FaturaAno,FaturaPaga")] Transacao transacao)
    {
        if (id != transacao.Id) return NotFound();

        if (ModelState.IsValid)
        {
            transacao.UsuarioId = UsuarioId;
            var resultado = await _transacaoService.AtualizarAsync(transacao);
            TempData["Sucesso"] = resultado.Mensagem;
            return RedirectToAction(nameof(Index));
        }

        await CarregarSelectLists(transacao.CategoriaId, transacao.CartaoCreditoId, transacao.ContaBancariaId);
        return View(transacao);
    }

    // GET: Transacoes/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var transacao = await _transacaoService.ObterPorIdAsync(id.Value, incluirRelacionamentos: true, usuarioId: UsuarioId);
        if (transacao == null) return NotFound();

        return View(transacao);
    }

    // POST: Transacoes/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var resultado = await _transacaoService.ExcluirAsync(id, UsuarioId);
        if (resultado.Sucesso)
            TempData["Sucesso"] = resultado.Mensagem;

        return RedirectToAction(nameof(Index));
    }

    private async Task CarregarSelectLists(int? categoriaId = null, int? cartaoId = null, int? contaId = null)
    {
        ViewBag.CategoriaId = new SelectList(await _transacaoService.ListarCategoriasParaSelecaoAsync(UsuarioId), "Id", "Nome", categoriaId);
        ViewBag.CartaoCreditoId = new SelectList(await _transacaoService.ListarCartoesParaSelecaoAsync(UsuarioId), "Id", "Nome", cartaoId);
        ViewBag.ContaBancariaId = new SelectList(await _transacaoService.ListarContasParaSelecaoAsync(UsuarioId), "Id", "Nome", contaId);
    }
}
