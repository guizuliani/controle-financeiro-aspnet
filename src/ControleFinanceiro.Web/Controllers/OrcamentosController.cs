using ControleFinanceiro.Application.Ports;
using ControleFinanceiro.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ControleFinanceiro.Controllers;

public class OrcamentosController : BaseAuthController
{
    private readonly IOrcamentoService _orcamentoService;
    private readonly ICategoriaService _categoriaService;

    public OrcamentosController(IOrcamentoService orcamentoService, ICategoriaService categoriaService)
    {
        _orcamentoService = orcamentoService;
        _categoriaService = categoriaService;
    }

    public async Task<IActionResult> Index(int? mes, int? ano)
    {
        var hoje = DateTime.Today;
        int mesFiltro = mes ?? hoje.Month;
        int anoFiltro = ano ?? hoje.Year;

        var orcamentos = await _orcamentoService.ListarResumosMesAsync(mesFiltro, anoFiltro, UsuarioId);
        var categorias = await _categoriaService.ListarTodasAsync(usuarioId: UsuarioId);

        ViewBag.Mes = mesFiltro;
        ViewBag.Ano = anoFiltro;
        ViewBag.Categorias = new SelectList(categorias, "Id", "Nome");

        return View(orcamentos);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Salvar(int categoriaId, decimal valorLimite, int mes, int ano)
    {
        if (valorLimite <= 0)
        {
            TempData["Erro"] = "O valor limite do orçamento deve ser maior que zero.";
            return RedirectToAction(nameof(Index), new { mes, ano });
        }

        var orcamento = new OrcamentoCategoria
        {
            UsuarioId = UsuarioId,
            CategoriaId = categoriaId,
            ValorLimite = valorLimite,
            Mes = mes,
            Ano = ano
        };

        var resultado = await _orcamentoService.SalvarAsync(orcamento);
        TempData[resultado.Sucesso ? "Sucesso" : "Erro"] = resultado.Mensagem;
        return RedirectToAction(nameof(Index), new { mes, ano });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Excluir(int id, int mes, int ano)
    {
        var resultado = await _orcamentoService.ExcluirAsync(id, UsuarioId);
        TempData[resultado.Sucesso ? "Sucesso" : "Erro"] = resultado.Mensagem;
        return RedirectToAction(nameof(Index), new { mes, ano });
    }
}
