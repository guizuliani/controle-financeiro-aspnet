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
        public async Task<IActionResult> Index(int? mes, int? ano, TipoTransacao? tipo, int? categoriaId, int? cartaoId, FormaPagamento? formaPagamento, string? busca)
        {
            var dataAtual = DateTime.Today;
            int mesFiltro = mes ?? dataAtual.Month;
            int anoFiltro = ano ?? dataAtual.Year;

            var query = _context.Transacoes
                .Include(t => t.Categoria)
                .Include(t => t.CartaoCredito)
                .AsQueryable();

            if (mes.HasValue)
                query = query.Where(t => t.Data.Month == mesFiltro);

            if (ano.HasValue)
                query = query.Where(t => t.Data.Year == anoFiltro);

            if (tipo.HasValue)
                query = query.Where(t => t.Tipo == tipo.Value);

            if (categoriaId.HasValue)
                query = query.Where(t => t.CategoriaId == categoriaId.Value);

            if (cartaoId.HasValue)
                query = query.Where(t => t.CartaoCreditoId == cartaoId.Value);

            if (formaPagamento.HasValue)
                query = query.Where(t => t.FormaPagamento == formaPagamento.Value);

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
            ViewBag.CartaoId = cartaoId;
            ViewBag.FormaPagamento = formaPagamento;
            ViewBag.Busca = busca;

            ViewBag.Categorias = new SelectList(await _context.Categorias.OrderBy(c => c.Nome).ToListAsync(), "Id", "Nome", categoriaId);
            ViewBag.Cartoes = new SelectList(await _context.CartoesCredito.OrderBy(c => c.Nome).ToListAsync(), "Id", "Nome", cartaoId);

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
        public async Task<IActionResult> Create([Bind("Descricao,Valor,Data,Tipo,CategoriaId,Observacao,FormaPagamento,CartaoCreditoId,TotalParcelas")] Transacao transacao)
        {
            if (transacao.FormaPagamento == FormaPagamento.CartaoCredito && (!transacao.CartaoCreditoId.HasValue || transacao.CartaoCreditoId == 0))
            {
                ModelState.AddModelError("CartaoCreditoId", "Selecione o cartão de crédito para esta forma de pagamento.");
            }

            if (ModelState.IsValid)
            {
                if (transacao.FormaPagamento == FormaPagamento.CartaoCredito && transacao.CartaoCreditoId.HasValue)
                {
                    var cartao = await _context.CartoesCredito.FindAsync(transacao.CartaoCreditoId.Value);
                    if (cartao != null)
                    {
                        int parcelas = Math.Max(1, transacao.TotalParcelas);
                        decimal valorParcela = Math.Round(transacao.Valor / parcelas, 2);
                        decimal diferencaCentavos = transacao.Valor - (valorParcela * parcelas);
                        Guid compraId = Guid.NewGuid();

                        // Determinar mês inicial da fatura com base no dia de fechamento
                        DateTime dataRef = transacao.Data;
                        if (dataRef.Day >= cartao.DiaFechamento)
                        {
                            dataRef = new DateTime(dataRef.Year, dataRef.Month, 1).AddMonths(1);
                        }

                        for (int i = 1; i <= parcelas; i++)
                        {
                            var mesFaturaData = dataRef.AddMonths(i - 1);
                            decimal valorAtual = (i == parcelas) ? valorParcela + diferencaCentavos : valorParcela;
                            decimal pontos = CalcularPontos(valorAtual, cartao);

                            var parcelaTransacao = new Transacao
                            {
                                Descricao = parcelas > 1 ? $"{transacao.Descricao} ({i}/{parcelas})" : transacao.Descricao,
                                Valor = valorAtual,
                                Data = transacao.Data,
                                Tipo = TipoTransacao.Despesa, // Compra no cartão é sempre despesa
                                CategoriaId = transacao.CategoriaId,
                                Observacao = transacao.Observacao,
                                FormaPagamento = FormaPagamento.CartaoCredito,
                                CartaoCreditoId = cartao.Id,
                                NumeroParcela = i,
                                TotalParcelas = parcelas,
                                CompraId = compraId,
                                FaturaMes = mesFaturaData.Month,
                                FaturaAno = mesFaturaData.Year,
                                PontosGerados = pontos,
                                FaturaPaga = false
                            };

                            _context.Transacoes.Add(parcelaTransacao);
                        }

                        await _context.SaveChangesAsync();
                        TempData["Sucesso"] = parcelas > 1 
                            ? $"Compra parcelada em {parcelas}x cadastrada com sucesso!" 
                            : "Transação de cartão cadastrada com sucesso!";
                        return RedirectToAction(nameof(Index));
                    }
                }

                // Transação comum à vista (Conta Corrente)
                transacao.NumeroParcela = 1;
                transacao.TotalParcelas = 1;
                transacao.PontosGerados = 0;
                transacao.CartaoCreditoId = null;
                _context.Add(transacao);
                await _context.SaveChangesAsync();
                TempData["Sucesso"] = "Transação cadastrada com sucesso!";
                return RedirectToAction(nameof(Index));
            }

            await CarregarSelectLists(transacao.CategoriaId, transacao.CartaoCreditoId);
            return View(transacao);
        }

        // GET: Transacoes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var transacao = await _context.Transacoes.FindAsync(id);
            if (transacao == null) return NotFound();

            await CarregarSelectLists(transacao.CategoriaId, transacao.CartaoCreditoId);
            return View(transacao);
        }

        // POST: Transacoes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Descricao,Valor,Data,Tipo,CategoriaId,Observacao,FormaPagamento,CartaoCreditoId,NumeroParcela,TotalParcelas,FaturaMes,FaturaAno,FaturaPaga")] Transacao transacao)
        {
            if (id != transacao.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    if (transacao.FormaPagamento == FormaPagamento.CartaoCredito && transacao.CartaoCreditoId.HasValue)
                    {
                        var cartao = await _context.CartoesCredito.FindAsync(transacao.CartaoCreditoId.Value);
                        if (cartao != null)
                        {
                            transacao.PontosGerados = CalcularPontos(transacao.Valor, cartao);
                        }
                    }
                    else
                    {
                        transacao.CartaoCreditoId = null;
                        transacao.PontosGerados = 0;
                    }

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

            await CarregarSelectLists(transacao.CategoriaId, transacao.CartaoCreditoId);
            return View(transacao);
        }

        // GET: Transacoes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var transacao = await _context.Transacoes
                .Include(t => t.Categoria)
                .Include(t => t.CartaoCredito)
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

        private async Task CarregarSelectLists(int? categoriaId = null, int? cartaoId = null)
        {
            ViewBag.CategoriaId = new SelectList(await _context.Categorias.OrderBy(c => c.Nome).ToListAsync(), "Id", "Nome", categoriaId);
            ViewBag.CartaoCreditoId = new SelectList(await _context.CartoesCredito.OrderBy(c => c.Nome).ToListAsync(), "Id", "Nome", cartaoId);
        }

        private static decimal CalcularPontos(decimal valor, CartaoCredito cartao)
        {
            if (!cartao.AcumulaPontos || cartao.FatorPontuacao <= 0)
                return 0;

            return cartao.TipoRegra switch
            {
                TipoRegraPontos.PorDolar => Math.Round((valor / (cartao.CotacaoDolarReferencia > 0 ? cartao.CotacaoDolarReferencia : 5.50m)) * cartao.FatorPontuacao, 2),
                TipoRegraPontos.PorReal => Math.Round(valor * cartao.FatorPontuacao, 2),
                TipoRegraPontos.Cashback => Math.Round(valor * (cartao.FatorPontuacao / 100m), 2),
                _ => 0
            };
        }
    }
}
