using Microsoft.AspNetCore.Mvc;
using ControleFinanceiro.Application.Ports;
using ControleFinanceiro.Domain.Entities;

namespace ControleFinanceiro.Controllers;

public class CategoriasController : BaseAuthController
{
    private readonly ICategoriaService _categoriaService;

    public CategoriasController(ICategoriaService categoriaService)
    {
        _categoriaService = categoriaService;
    }

    // GET: Categorias
    public async Task<IActionResult> Index()
    {
        var categorias = await _categoriaService.ListarTodasAsync(incluirTransacoes: true, usuarioId: UsuarioId);
        return View(categorias);
    }

    // GET: Categorias/Create
    public IActionResult Create() => View();

    // POST: Categorias/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Nome,CorHex,Icone")] Categoria categoria)
    {
        if (ModelState.IsValid)
        {
            categoria.UsuarioId = UsuarioId;
            var resultado = await _categoriaService.CriarAsync(categoria);
            TempData["Sucesso"] = resultado.Mensagem;
            return RedirectToAction(nameof(Index));
        }
        return View(categoria);
    }

    // GET: Categorias/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var categoria = await _categoriaService.ObterPorIdAsync(id.Value, usuarioId: UsuarioId);
        if (categoria == null) return NotFound();

        return View(categoria);
    }

    // POST: Categorias/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Nome,CorHex,Icone")] Categoria categoria)
    {
        if (id != categoria.Id) return NotFound();

        if (ModelState.IsValid)
        {
            categoria.UsuarioId = UsuarioId;
            var resultado = await _categoriaService.AtualizarAsync(categoria);
            TempData["Sucesso"] = resultado.Mensagem;
            return RedirectToAction(nameof(Index));
        }
        return View(categoria);
    }

    // GET: Categorias/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var categoria = await _categoriaService.ObterPorIdAsync(id.Value, incluirTransacoes: true, usuarioId: UsuarioId);
        if (categoria == null) return NotFound();

        return View(categoria);
    }

    // POST: Categorias/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var resultado = await _categoriaService.ExcluirAsync(id, UsuarioId);
        TempData[resultado.Sucesso ? "Sucesso" : "Erro"] = resultado.Mensagem;
        return RedirectToAction(nameof(Index));
    }
}
