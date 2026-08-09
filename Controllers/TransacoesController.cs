using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ControleFinanceiro.Data;
using ControleFinanceiro.Models;
using ControleFinanceiro.Models.Enums;

namespace ControleFinanceiro.Controllers
{
    public class TransacoesController : Controller
    {
        private readonly AppDbContext _context;

        public TransacoesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Transacoes
        public async Task<IActionResult> Index(int? mes, int? ano, TipoTransacao? tipo, int? categoriaId, string? busca)
        {
            var dataAtual = DateTime.Today;
            int mesFiltro = mes ?? dataAtual.Month;
            int anoFiltro = ano ?? dataAtual.Year;

            var query = _context.Transacoes
                .Include(t => t.Categoria)
                .AsQueryable();

            if (mes.HasValue)
                query = query.Where(t => t.Data.Month == mesFiltro);

            if (ano.HasValue)
                query = query.Where(t => t.Data.Year == anoFiltro);

            if (tipo.HasValue)
                query = query.Where(t => t.Tipo == tipo.Value);

            if (categoriaId.HasValue)
                query = query.Where(t => t.CategoriaId == categoriaId.Value);

            if (!string.IsNullOrWhiteSpace(busca))
                query = query.Where(t => t.Descricao.Contains(busca) || (t.Observacao != null && t.Observacao.Contains(busca)));

            var transacoes = await query
                .OrderByDescending(t => t.Data)
                .ThenByDescending(t => t.Id)
                .ToListAsync();

            ViewBag.Mes = mesFiltro;
            ViewBag.Ano = anoFiltro;
            ViewBag.Tipo = tipo;
            ViewBag.CategoriaId = categoriaId;
            ViewBag.Busca = busca;

            ViewBag.Categorias = new SelectList(await _context.Categorias.OrderBy(c => c.Nome).ToListAsync(), "Id", "Nome", categoriaId);

            return View(transacoes);
        }

        // GET: Transacoes/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.CategoriaId = new SelectList(await _context.Categorias.OrderBy(c => c.Nome).ToListAsync(), "Id", "Nome");
            return View(new Transacao { Data = DateTime.Today });
        }

        // POST: Transacoes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Descricao,Valor,Data,Tipo,CategoriaId,Observacao")] Transacao transacao)
        {
            if (ModelState.IsValid)
            {
                _context.Add(transacao);
                await _context.SaveChangesAsync();
                TempData["Sucesso"] = "Transação cadastrada com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.CategoriaId = new SelectList(await _context.Categorias.OrderBy(c => c.Nome).ToListAsync(), "Id", "Nome", transacao.CategoriaId);
            return View(transacao);
        }

        // GET: Transacoes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var transacao = await _context.Transacoes.FindAsync(id);
            if (transacao == null) return NotFound();

            ViewBag.CategoriaId = new SelectList(await _context.Categorias.OrderBy(c => c.Nome).ToListAsync(), "Id", "Nome", transacao.CategoriaId);
            return View(transacao);
        }

        // POST: Transacoes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Descricao,Valor,Data,Tipo,CategoriaId,Observacao")] Transacao transacao)
        {
            if (id != transacao.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(transacao);
                    await _context.SaveChangesAsync();
                    TempData["Sucesso"] = "Transação atualizada com sucesso!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Transacoes.Any(e => e.Id == transacao.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.CategoriaId = new SelectList(await _context.Categorias.OrderBy(c => c.Nome).ToListAsync(), "Id", "Nome", transacao.CategoriaId);
            return View(transacao);
        }

        // GET: Transacoes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var transacao = await _context.Transacoes
                .Include(t => t.Categoria)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (transacao == null) return NotFound();

            return View(transacao);
        }

        // POST: Transacoes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var transacao = await _context.Transacoes.FindAsync(id);
            if (transacao != null)
            {
                _context.Transacoes.Remove(transacao);
                await _context.SaveChangesAsync();
                TempData["Sucesso"] = "Transação excluída com sucesso!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
