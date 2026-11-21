using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Models.ViewModels;
using Xunit;

namespace ControleFinanceiro.Tests;

public class LimiteCartaoTests
{
    [Fact]
    public void LimiteDisponivel_DeveDeduzirApenasTransacoesNaoPagas()
    {
        var cartao = new CartaoCredito
        {
            Id = 1,
            Nome = "Visa Platinum",
            LimiteTotal = 5000.00m,
            Transacoes = new List<Transacao>
            {
                new() { Valor = 1000.00m, CartaoCreditoId = 1, FaturaPaga = false },
                new() { Valor = 500.00m, CartaoCreditoId = 1, FaturaPaga = false },
                new() { Valor = 2000.00m, CartaoCreditoId = 1, FaturaPaga = true }
            }
        };

        Assert.Equal(1500.00m, cartao.CalcularLimiteUtilizado());
        Assert.Equal(3500.00m, cartao.CalcularLimiteDisponivel());
    }

    [Fact]
    public void PercentualUso_DeveCalcularCorretamente()
    {
        var resumo = new CartaoResumoItem
        {
            LimiteTotal = 10000.00m,
            LimiteUtilizado = 2500.00m
        };

        Assert.Equal(25.0m, resumo.PercentualUso);
        Assert.Equal(7500.00m, resumo.LimiteDisponivel);
    }

    [Fact]
    public void PercentualUso_LimiteTotalZero_NaoDeveDarDivisaoPorZero()
    {
        var resumo = new CartaoResumoItem
        {
            LimiteTotal = 0m,
            LimiteUtilizado = 0m
        };

        Assert.Equal(0m, resumo.PercentualUso);
    }
}
