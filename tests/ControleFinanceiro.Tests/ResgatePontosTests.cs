using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Models.ViewModels;
using Xunit;

namespace ControleFinanceiro.Tests;

public class ResgatePontosTests
{
    [Fact]
    public void SaldoPontos_DeveSubtrairResgatesDoTotalAcumulado()
    {
        var programa = new ProgramaPontosItem
        {
            NomePrograma = "Livelo",
            TotalAcumulado = 25000m,
            TotalResgatado = 10000m
        };

        Assert.Equal(15000m, programa.SaldoAtual);
    }

    [Fact]
    public void SaldoPontos_CartaoDominio_DeveCalcularSaldoLiquido()
    {
        var cartao = new CartaoCredito
        {
            Transacoes = new List<Transacao>
            {
                new() { PontosGerados = 1500m },
                new() { PontosGerados = 500m }
            },
            Resgates = new List<ResgatePontos>
            {
                new() { Pontos = 800m }
            }
        };

        Assert.Equal(1200m, cartao.CalcularSaldoPontos());
    }

    [Fact]
    public void SaldoPontos_SeResgateMaiorQueAcumulado_SaldoDeveSerZero()
    {
        var programa = new ProgramaPontosItem
        {
            NomePrograma = "Esfera",
            TotalAcumulado = 5000m,
            TotalResgatado = 6000m
        };

        Assert.Equal(0m, programa.SaldoAtual);
    }

    [Fact]
    public void SaldoGeralPontos_DeveSomarTodosOsProgramas()
    {
        var viewModel = new PontosViewModel
        {
            Programas = new List<ProgramaPontosItem>
            {
                new() { TotalAcumulado = 10000m, TotalResgatado = 2000m },
                new() { TotalAcumulado = 5000m, TotalResgatado = 1000m }
            }
        };

        Assert.Equal(12000m, viewModel.SaldoTotalGeral);
    }
}
