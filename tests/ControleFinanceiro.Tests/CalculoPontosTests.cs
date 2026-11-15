using ControleFinanceiro.Models;
using ControleFinanceiro.Models.Enums;
using Xunit;

namespace ControleFinanceiro.Tests
{
    public class CalculoPontosTests
    {
        [Fact]
        public void CalcularPontos_RegraPorDolar_DeveCalcularCorretamente()
        {
            // Cenário: R$ 550,00 gastos, Dólar a R$ 5,50, Multiplicador 2.5 pts/US$
            // US$ 550 / 5.50 = US$ 100 * 2.5 = 250 pontos
            var cartao = new CartaoCredito
            {
                AcumulaPontos = true,
                TipoRegra = TipoRegraPontos.PorDolar,
                CotacaoDolarReferencia = 5.50m,
                FatorPontuacao = 2.5m
            };

            decimal valorGasto = 550.00m;
            decimal pontos = CalcularPontosHelper(valorGasto, cartao);

            Assert.Equal(250.00m, pontos);
        }

        [Fact]
        public void CalcularPontos_RegraPorReal_DeveMultiplicarDireto()
        {
            // Cenário: R$ 350,00 gastos, 1.5 pontos por Real gasto = 525 pontos
            var cartao = new CartaoCredito
            {
                AcumulaPontos = true,
                TipoRegra = TipoRegraPontos.PorReal,
                FatorPontuacao = 1.5m
            };

            decimal valorGasto = 350.00m;
            decimal pontos = CalcularPontosHelper(valorGasto, cartao);

            Assert.Equal(525.00m, pontos);
        }

        [Fact]
        public void CalcularPontos_RegraCashback_DeveCalcularPercentual()
        {
            // Cenário: R$ 1.000,00 gastos, 1.5% de cashback = R$ 15,00
            var cartao = new CartaoCredito
            {
                AcumulaPontos = true,
                TipoRegra = TipoRegraPontos.Cashback,
                FatorPontuacao = 1.5m
            };

            decimal valorGasto = 1000.00m;
            decimal cashback = CalcularPontosHelper(valorGasto, cartao);

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

            decimal pontos = CalcularPontosHelper(1000.00m, cartao);

            Assert.Equal(0m, pontos);
        }

        private static decimal CalcularPontosHelper(decimal valor, CartaoCredito cartao)
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
