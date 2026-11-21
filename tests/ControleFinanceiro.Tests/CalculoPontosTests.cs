using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Enums;
using Xunit;

namespace ControleFinanceiro.Tests;

public class CalculoPontosTests
{
    [Fact]
    public void CalcularPontos_RegraPorDolar_DeveCalcularCorretamente()
    {
        var cartao = new CartaoCredito
        {
            AcumulaPontos = true,
            TipoRegra = TipoRegraPontos.PorDolar,
            CotacaoDolarReferencia = 5.50m,
            FatorPontuacao = 2.5m
        };

        decimal pontos = cartao.CalcularPontos(550.00m);

        Assert.Equal(250.00m, pontos);
    }

    [Fact]
    public void CalcularPontos_RegraPorReal_DeveMultiplicarDireto()
    {
        var cartao = new CartaoCredito
        {
            AcumulaPontos = true,
            TipoRegra = TipoRegraPontos.PorReal,
            FatorPontuacao = 1.5m
        };

        decimal pontos = cartao.CalcularPontos(350.00m);

        Assert.Equal(525.00m, pontos);
    }

    [Fact]
    public void CalcularPontos_RegraCashback_DeveCalcularPercentual()
    {
        var cartao = new CartaoCredito
        {
            AcumulaPontos = true,
            TipoRegra = TipoRegraPontos.Cashback,
            FatorPontuacao = 1.5m
        };

        decimal cashback = cartao.CalcularPontos(1000.00m);

        Assert.Equal(15.00m, cashback);
    }

    [Fact]
    public void CalcularPontos_CartaoSemPontuacao_DeveRetornarZero()
    {
        var cartao = new CartaoCredito
        {
            AcumulaPontos = false,
            TipoRegra = TipoRegraPontos.PorDolar,
            FatorPontuacao = 2.5m
        };

        decimal pontos = cartao.CalcularPontos(1000.00m);

        Assert.Equal(0m, pontos);
    }
}
