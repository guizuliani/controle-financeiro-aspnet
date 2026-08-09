using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ControleFinanceiro.Data;
using ControleFinanceiro.Models;

namespace ControleFinanceiro.Controllers
{
    public class CategoriasController : Controller
    {
        private readonly AppDbContext _context;

        public CategoriasController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Categorias
        public async Task<IActionResult> Index()
        {
            var categorias = await _context.Categorias
                .Include(c => c.Transacoes)
                .OrderBy(c => c.Nome)
                .ToListAsync();

            return View(categorias);
        }

        // GET: Categorias/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Categorias/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nome,CorHex,Icone")] Categoria categoria)
        {
            if (ModelState.IsValid)
            {
                _context.Add(categoria);
                await _context.SaveChangesAsync();
                TempData["Sucesso"] = "Categoria criada com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            return View(categoria);
        }

        // GET: Categorias/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var categoria = await _context.Categorias.FindAsync(id);
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
                try
                {
                    _context.Update(categoria);
                    await _context.SaveChangesAsync();
                    TempData["Sucesso"] = "Categoria atualizada com sucesso!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Categorias.Any(e => e.Id == categoria.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(categoria);
        }

        // GET: Categorias/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var categoria = await _context.Categorias
                .Include(c => c.Transacoes)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (categoria == null) return NotFound();

            return View(categoria);
        }

        // POST: Categorias/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var categoria = await _context.Categorias
                .Include(c => c.Transacoes)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (categoria != null)
            {
                if (categoria.Transacoes.Any())
                {
                    TempData["Erro"] = "Não é possível excluir uma categoria que possui transações vinculadas.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Categorias.Remove(categoria);
                await _context.SaveChangesAsync();
                TempData["Sucesso"] = "Categoria excluída com sucesso!";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
