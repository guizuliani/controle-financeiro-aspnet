using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using ControleFinanceiro.Application.Services;
using ControleFinanceiro.Controllers;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Infrastructure.Persistence;
using ControleFinanceiro.Infrastructure.Repositories;
using Xunit;

namespace ControleFinanceiro.Tests;

public class TransacoesControllerIntegrationTests
{
    private static AppDbContext CriarContextoEmMemoria(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        var context = new AppDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    private static TransacoesController CriarTransacoesController(AppDbContext context)
    {
        var service = new TransacaoService(
            new TransacaoRepository(context),
            new CartaoCreditoRepository(context),
            new CategoriaRepository(context),
            new ContaBancariaRepository(context),
            new UnitOfWork(context));

        var controller = new TransacoesController(service);
        ConfigurarTempData(controller);
        return controller;
    }

    private static CartoesController CriarCartoesController(AppDbContext context)
    {
        var service = new CartaoService(
            new CartaoCreditoRepository(context),
            new TransacaoRepository(context),
            new CategoriaRepository(context),
            new UnitOfWork(context));

        var controller = new CartoesController(service);
        ConfigurarTempData(controller);
        return controller;
    }

    private static void ConfigurarTempData(Controller controller)
    {
        var httpContext = new DefaultHttpContext();
        controller.TempData = new TempDataDictionary(httpContext, new FakeTempDataProvider());
    }

    private class FakeTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();
        public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
    }

    [Fact]
    public async Task CriarTransacao_ContaCorrente_DeveSalvarUmaTransacaoSemPontos()
    {
        using var context = CriarContextoEmMemoria("Db_Transacao_ContaCorrente");
        var controller = CriarTransacoesController(context);

        var transacao = new Transacao
        {
            Descricao = "Salário Mensal",
            Valor = 5000.00m,
            Data = DateTime.Today,
            Tipo = TipoTransacao.Receita,
            FormaPagamento = FormaPagamento.ContaCorrente,
            CategoriaId = 1
        };

        var result = await controller.Create(transacao);

        var redirecionamento = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirecionamento.ActionName);

        var salva = await context.Transacoes.FirstOrDefaultAsync(t => t.Descricao == "Salário Mensal");
        Assert.NotNull(salva);
        Assert.Equal(5000.00m, salva.Valor);
        Assert.Equal(FormaPagamento.ContaCorrente, salva.FormaPagamento);
        Assert.Equal(0m, salva.PontosGerados);
        Assert.Null(salva.CartaoCreditoId);
    }

    [Fact]
    public async Task CriarTransacao_CartaoParcelado_DeveGerarMultiplasParcelasComPontosEFaturas()
    {
        using var context = CriarContextoEmMemoria("Db_Transacao_CartaoParcelado");

        var cartao = new CartaoCredito
        {
            Id = 10,
            Nome = "Cartão Black Teste",
            LimiteTotal = 20000m,
            DiaFechamento = 15,
            DiaVencimento = 25,
            AcumulaPontos = true,
            TipoRegra = TipoRegraPontos.PorDolar,
            FatorPontuacao = 2.0m,
            CotacaoDolarReferencia = 5.00m
        };
        context.CartoesCredito.Add(cartao);
        await context.SaveChangesAsync();

        var controller = CriarTransacoesController(context);

        var transacao = new Transacao
        {
            Descricao = "Notebook",
            Valor = 600.00m,
            Data = new DateTime(2026, 3, 10),
            Tipo = TipoTransacao.Despesa,
            FormaPagamento = FormaPagamento.CartaoCredito,
            CartaoCreditoId = 10,
            TotalParcelas = 3,
            CategoriaId = 5
        };

        var result = await controller.Create(transacao);

        var redirecionamento = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirecionamento.ActionName);

        var parcelas = await context.Transacoes
            .Where(t => t.CartaoCreditoId == 10)
            .OrderBy(t => t.NumeroParcela)
            .ToListAsync();

        Assert.Equal(3, parcelas.Count);

        Assert.Equal(1, parcelas[0].NumeroParcela);
        Assert.Equal(3, parcelas[0].FaturaMes);
        Assert.Equal(200.00m, parcelas[0].Valor);
        Assert.Equal(80.00m, parcelas[0].PontosGerados);

        Assert.Equal(2, parcelas[1].NumeroParcela);
        Assert.Equal(4, parcelas[1].FaturaMes);
        Assert.Equal(200.00m, parcelas[1].Valor);

        Assert.Equal(3, parcelas[2].NumeroParcela);
        Assert.Equal(5, parcelas[2].FaturaMes);
        Assert.Equal(200.00m, parcelas[2].Valor);

        Assert.NotNull(parcelas[0].CompraId);
        Assert.Equal(parcelas[0].CompraId, parcelas[1].CompraId);
    }

    [Fact]
    public async Task PagarFatura_DeveQuitarTransacoesECriarDespesaConsolidada()
    {
        using var context = CriarContextoEmMemoria("Db_PagarFatura");

        var cartao = new CartaoCredito
        {
            Id = 20,
            Nome = "Nubank Teste",
            LimiteTotal = 5000m,
            DiaFechamento = 10,
            DiaVencimento = 20
        };
        context.CartoesCredito.Add(cartao);

        context.Transacoes.AddRange(
            new Transacao
            {
                Descricao = "Mercado",
                Valor = 300m,
                Data = new DateTime(2026, 5, 5),
                CartaoCreditoId = 20,
                FaturaMes = 5,
                FaturaAno = 2026,
                FaturaPaga = false,
                CategoriaId = 2,
                FormaPagamento = FormaPagamento.CartaoCredito
            },
            new Transacao
            {
                Descricao = "Farmácia",
                Valor = 150m,
                Data = new DateTime(2026, 5, 8),
                CartaoCreditoId = 20,
                FaturaMes = 5,
                FaturaAno = 2026,
                FaturaPaga = false,
                CategoriaId = 6,
                FormaPagamento = FormaPagamento.CartaoCredito
            }
        );
        await context.SaveChangesAsync();

        var cartoesController = CriarCartoesController(context);

        var result = await cartoesController.PagarFatura(20, 5, 2026);

        var redirecionamento = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Fatura", redirecionamento.ActionName);

        var transacoesFatura = await context.Transacoes
            .Where(t => t.CartaoCreditoId == 20 && t.FaturaMes == 5 && t.FaturaAno == 2026)
            .ToListAsync();

        Assert.All(transacoesFatura, t => Assert.True(t.FaturaPaga));

        var pagamentoConsolidado = await context.Transacoes
            .FirstOrDefaultAsync(t => t.FormaPagamento == FormaPagamento.ContaCorrente && t.Descricao.Contains("Pagamento Fatura"));

        Assert.NotNull(pagamentoConsolidado);
        Assert.Equal(450m, pagamentoConsolidado.Valor);
        Assert.Equal(TipoTransacao.Despesa, pagamentoConsolidado.Tipo);
    }
}
