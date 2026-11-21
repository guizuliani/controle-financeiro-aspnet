using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Enums;
using Xunit;

namespace ControleFinanceiro.Tests;

public class FaturaECorteCartaoTests
{
    [Fact]
    public void CompraAntesDoFechamento_DeveCairNaFaturaDoMesAtual()
    {
        var cartao = new CartaoCredito { DiaFechamento = 20 };
        var dataCompra = new DateTime(2026, 3, 15);

        DateTime mesFatura = cartao.CalcularMesInicialFatura(dataCompra);

        Assert.Equal(3, mesFatura.Month);
        Assert.Equal(2026, mesFatura.Year);
    }

    [Fact]
    public void CompraNoDiaOuAposFechamento_DeveVirarParaProximoMes()
    {
        var cartao = new CartaoCredito { DiaFechamento = 20 };
        var dataCompra = new DateTime(2026, 3, 20);

        DateTime mesFatura = cartao.CalcularMesInicialFatura(dataCompra);

        Assert.Equal(4, mesFatura.Month);
        Assert.Equal(2026, mesFatura.Year);
    }

    [Fact]
    public void CompraParcelada_DivisaoDeCentavos_DeveBaterValorTotalExato()
    {
        var cartao = new CartaoCredito { Id = 1, DiaFechamento = 20 };
        var compraBase = new Transacao
        {
            Descricao = "Compra Teste",
            Valor = 100.00m,
            Data = new DateTime(2026, 3, 10),
            TotalParcelas = 3,
            CategoriaId = 1,
            FormaPagamento = FormaPagamento.CartaoCredito
        };

        var parcelas = Transacao.GerarParcelasCartao(compraBase, cartao);

        Assert.Equal(3, parcelas.Count);
        Assert.Equal(33.33m, parcelas[0].Valor);
        Assert.Equal(33.33m, parcelas[1].Valor);
        Assert.Equal(33.34m, parcelas[2].Valor);
        Assert.Equal(100.00m, parcelas.Sum(p => p.Valor));
    }

    [Fact]
    public void CompraParcelada_DistribuicaoMeses_DeveAlocarMesesConsecutivos()
    {
        var cartao = new CartaoCredito { Id = 1, DiaFechamento = 20 };
        var compraBase = new Transacao
        {
            Descricao = "Compra Teste",
            Valor = 300.00m,
            Data = new DateTime(2026, 11, 10),
            TotalParcelas = 3,
            CategoriaId = 1,
            FormaPagamento = FormaPagamento.CartaoCredito
        };

        var parcelas = Transacao.GerarParcelasCartao(compraBase, cartao);

        Assert.Equal((11, 2026), (parcelas[0].FaturaMes!.Value, parcelas[0].FaturaAno!.Value));
        Assert.Equal((12, 2026), (parcelas[1].FaturaMes!.Value, parcelas[1].FaturaAno!.Value));
        Assert.Equal((1, 2027), (parcelas[2].FaturaMes!.Value, parcelas[2].FaturaAno!.Value));
    }
}
