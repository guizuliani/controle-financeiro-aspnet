using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ControleFinanceiro.Data;
using ControleFinanceiro.Models;
using ControleFinanceiro.Models.Enums;
using ControleFinanceiro.Models.ViewModels;

namespace ControleFinanceiro.Controllers
{
    public class CartoesController : Controller
    {
        private readonly AppDbContext _context;

        public CartoesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Cartoes
        public async Task<IActionResult> Index()
        {
            var cartoes = await _context.CartoesCredito
                .Include(c => c.Transacoes)
                .OrderBy(c => c.Nome)
                .ToListAsync();

            var dataAtual = DateTime.Today;

            var viewModelList = cartoes.Select(c =>
            {
                var transacoesNaoPagas = c.Transacoes.Where(t => !t.FaturaPaga).ToList();
                var limiteUtilizado = transacoesNaoPagas.Sum(t => t.Valor);
                var faturaMesAtual = c.Transacoes
                    .Where(t => t.FaturaMes == dataAtual.Month && t.FaturaAno == dataAtual.Year)
                    .Sum(t => t.Valor);

                return new CartaoResumoItem
                {
                    Id = c.Id,
                    Nome = c.Nome,
                    CorHex = c.CorHex,
                    LimiteTotal = c.LimiteTotal,
                    LimiteUtilizado = limiteUtilizado,
                    FaturaMesAtual = faturaMesAtual,
                    DiaFechamento = c.DiaFechamento,
                    DiaVencimento = c.DiaVencimento,
                    NomeProgramaPontos = c.AcumulaPontos ? (c.NomeProgramaPontos ?? "Pontos") : "Sem Pontos"
                };
            }).ToList();

            return View(viewModelList);
        }

        // GET: Cartoes/Create
        public IActionResult Create()
        {
            return View(new CartaoCredito());
        }

        // POST: Cartoes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nome,Bandeira,CorHex,LimiteTotal,DiaFechamento,DiaVencimento,AcumulaPontos,NomeProgramaPontos,TipoRegra,FatorPontuacao,CotacaoDolarReferencia")] CartaoCredito cartao)
        {
            if (ModelState.IsValid)
            {
                _context.Add(cartao);
                await _context.SaveChangesAsync();
                TempData["Sucesso"] = "Cartão de crédito cadastrado com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            return View(cartao);
        }

        // GET: Cartoes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var cartao = await _context.CartoesCredito.FindAsync(id);
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
                try
                {
                    _context.Update(cartao);
                    await _context.SaveChangesAsync();
                    TempData["Sucesso"] = "Cartão de crédito atualizado com sucesso!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.CartoesCredito.Any(e => e.Id == cartao.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(cartao);
        }

        // GET: Cartoes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var cartao = await _context.CartoesCredito
                .Include(c => c.Transacoes)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (cartao == null) return NotFound();

            return View(cartao);
        }

        // POST: Cartoes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cartao = await _context.CartoesCredito
                .Include(c => c.Transacoes)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cartao != null)
            {
                if (cartao.Transacoes.Any())
                {
                    TempData["Erro"] = "Não é possível excluir um cartão que possui transações vinculadas.";
                    return RedirectToAction(nameof(Index));
                }

                _context.CartoesCredito.Remove(cartao);
                await _context.SaveChangesAsync();
                TempData["Sucesso"] = "Cartão de crédito excluído com sucesso!";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Cartoes/Fatura/5
        public async Task<IActionResult> Fatura(int id, int? mes, int? ano)
        {
            var cartao = await _context.CartoesCredito.FindAsync(id);
            if (cartao == null) return NotFound();

            var dataAtual = DateTime.Today;
            int mesFiltro = mes ?? dataAtual.Month;
            int anoFiltro = ano ?? dataAtual.Year;

            var transacoes = await _context.Transacoes
                .Include(t => t.Categoria)
                .Where(t => t.CartaoCreditoId == id && t.FaturaMes == mesFiltro && t.FaturaAno == anoFiltro)
                .OrderBy(t => t.Data)
                .ToListAsync();

            var viewModel = new FaturaViewModel
            {
                Cartao = cartao,
                Mes = mesFiltro,
                Ano = anoFiltro,
                Transacoes = transacoes
            };

            return View(viewModel);
        }

        // POST: Cartoes/PagarFatura
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PagarFatura(int id, int mes, int ano)
        {
            var cartao = await _context.CartoesCredito.FindAsync(id);
            if (cartao == null) return NotFound();

            var transacoes = await _context.Transacoes
                .Where(t => t.CartaoCreditoId == id && t.FaturaMes == mes && t.FaturaAno == ano && !t.FaturaPaga)
                .ToListAsync();

            if (!transacoes.Any())
            {
                TempData["Erro"] = "Esta fatura já está paga ou não possui lançamentos a quitar.";
                return RedirectToAction(nameof(Fatura), new { id, mes, ano });
            }

            decimal totalFatura = transacoes.Sum(t => t.Valor);

            foreach (var t in transacoes)
            {
                t.FaturaPaga = true;
            }

            // Registrar despesa consolidada de pagamento da fatura na conta corrente
            var categoriaOutros = await _context.Categorias.FirstOrDefaultAsync(c => c.Nome.Contains("Moradia") || c.Nome.Contains("Outros"));
            int categoriaId = categoriaOutros?.Id ?? 8;

            var pagamentoTransacao = new Transacao
            {
                Descricao = $"Pagamento Fatura {cartao.Nome} ({mes:D2}/{ano})",
                Valor = totalFatura,
                Data = DateTime.Today,
                Tipo = TipoTransacao.Despesa,
                FormaPagamento = FormaPagamento.ContaCorrente,
                CategoriaId = categoriaId,
                Observacao = $"Liquidação automática da fatura de {mes:D2}/{ano} do cartão {cartao.Nome}."
            };

            _context.Transacoes.Add(pagamentoTransacao);
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = $"Fatura de {mes:D2}/{ano} no valor de {totalFatura:C} paga com sucesso!";
            return RedirectToAction(nameof(Fatura), new { id, mes, ano });
        }
    }
}
