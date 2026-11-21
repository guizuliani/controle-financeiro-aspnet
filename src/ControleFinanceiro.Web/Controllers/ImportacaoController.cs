using System.Text;
using ControleFinanceiro.Application.Ports;
using ControleFinanceiro.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ControleFinanceiro.Controllers;

public class ImportacaoController : BaseAuthController
{
    private readonly IExtratoPortabilidadeService _portabilidadeService;
    private readonly ITransacaoService _transacaoService;

    public ImportacaoController(
        IExtratoPortabilidadeService portabilidadeService,
        ITransacaoService transacaoService)
    {
        _portabilidadeService = portabilidadeService;
        _transacaoService = transacaoService;
    }

    public async Task<IActionResult> Index()
    {
        await CarregarSelectLists();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Importar(
        IFormFile? arquivo,
        int categoriaId,
        FormaPagamento formaPagamento,
        int? contaBancariaId,
        int? cartaoCreditoId)
    {
        if (arquivo == null || arquivo.Length == 0)
        {
            TempData["Erro"] = "Selecione um arquivo CSV ou OFX válido.";
            return RedirectToAction(nameof(Index));
        }

        string conteudo;
        using (var reader = new StreamReader(arquivo.OpenReadStream(), Encoding.UTF8, detectEncodingFromByteOrderMarks: true))
        {
            conteudo = await reader.ReadToEndAsync();
        }

        var extensao = Path.GetExtension(arquivo.FileName).ToLowerInvariant();
        var itens = extensao == ".ofx"
            ? _portabilidadeService.LerArquivoOfx(conteudo)
            : _portabilidadeService.LerArquivoCsv(conteudo);

        var resultado = await _portabilidadeService.ImportarItensAsync(
            itens,
            categoriaId,
            formaPagamento,
            contaBancariaId,
            cartaoCreditoId,
            UsuarioId);

        TempData[resultado.Sucesso ? "Sucesso" : "Erro"] = resultado.Mensagem;
        return RedirectToAction(resultado.Sucesso ? "Index" : nameof(Index), resultado.Sucesso ? "Transacoes" : null);
    }

    [HttpGet]
    public async Task<IActionResult> ExportarCsv(int? mes, int? ano)
    {
        var transacoes = await _transacaoService.ListarFiltradasAsync(mes, ano, null, null, null, null, null, null, UsuarioId);
        var bytes = _portabilidadeService.ExportarCsv(transacoes);
        string nomeArquivo = $"extrato-financeiro-{DateTime.Today:yyyyMMdd}.csv";
        return File(bytes, "text/csv; charset=utf-8", nomeArquivo);
    }

    private async Task CarregarSelectLists()
    {
        ViewBag.Categorias = new SelectList(await _transacaoService.ListarCategoriasParaSelecaoAsync(UsuarioId), "Id", "Nome");
        ViewBag.Contas = new SelectList(await _transacaoService.ListarContasParaSelecaoAsync(UsuarioId), "Id", "Nome");
        ViewBag.Cartoes = new SelectList(await _transacaoService.ListarCartoesParaSelecaoAsync(UsuarioId), "Id", "Nome");
    }
}
