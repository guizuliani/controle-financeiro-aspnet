using ControleFinanceiro.Models;
using Xunit;

namespace ControleFinanceiro.Tests
{
    public class FaturaECorteCartaoTests
    {
        [Fact]
        public void CompraAntesDoFechamento_DeveCairNaFaturaDoMesAtual()
        {
            // Cartão fecha dia 20. Compra no dia 15 de Março
            int diaFechamento = 20;
            DateTime dataCompra = new DateTime(2026, 3, 15);

            DateTime mesFatura;
            if (dataCompra.Day >= diaFechamento)
            {
                mesFatura = new DateTime(dataCompra.Year, dataCompra.Month, 1).AddMonths(1);
            }
            else
            {
                mesFatura = dataCompra;
            }

            Assert.Equal(3, mesFatura.Month);
            Assert.Equal(2026, mesFatura.Year);
        }

        [Fact]
        public void CompraNoDiaOuAposFechamento_DeveVirarParaProximoMes()
        {
            // Cartão fecha dia 20. Compra no dia 20 de Março
            int diaFechamento = 20;
            DateTime dataCompra = new DateTime(2026, 3, 20);

            DateTime mesFatura;
            if (dataCompra.Day >= diaFechamento)
            {
                mesFatura = new DateTime(dataCompra.Year, dataCompra.Month, 1).AddMonths(1);
            }
            else
            {
                mesFatura = dataCompra;
            }

            Assert.Equal(4, mesFatura.Month);
            Assert.Equal(2026, mesFatura.Year);
        }

        [Fact]
        public void CompraParcelada_DivisaoDeCentavos_DeveBaterValorTotalExato()
        {
            // R$ 100,00 parcelado em 3x:
            // Parcela 1: 33.33
            // Parcela 2: 33.33
            // Parcela 3: 33.34
            // Soma = 100.00
            decimal valorTotal = 100.00m;
            int parcelas = 3;

            decimal valorParcela = Math.Round(valorTotal / parcelas, 2);
            decimal diferencaCentavos = valorTotal - (valorParcela * parcelas);

            var listaParcelas = new List<decimal>();
            for (int i = 1; i <= parcelas; i++)
            {
                decimal valorAtual = (i == parcelas) ? valorParcela + diferencaCentavos : valorParcela;
                listaParcelas.Add(valorAtual);
            }

            Assert.Equal(33.33m, listaParcelas[0]);
            Assert.Equal(33.33m, listaParcelas[1]);
            Assert.Equal(33.34m, listaParcelas[2]);
            Assert.Equal(valorTotal, listaParcelas.Sum());
        }

        [Fact]
        public void CompraParcelada_DistribuicaoMeses_DeveAlocarMesesConsecutivos()
        {
            // Compra no dia 10 de Novembro em 3x, cartão fecha dia 20
            // Parcela 1: Nov/2026
            // Parcela 2: Dez/2026
            // Parcela 3: Jan/2027
            DateTime dataCompra = new DateTime(2026, 11, 10);
            int diaFechamento = 20;
            int totalParcelas = 3;

            DateTime dataRef = dataCompra;
            if (dataRef.Day >= diaFechamento)
            {
                dataRef = new DateTime(dataRef.Year, dataRef.Month, 1).AddMonths(1);
            }

            var meses = new List<(int mes, int ano)>();
            for (int i = 1; i <= totalParcelas; i++)
            {
                var fatura = dataRef.AddMonths(i - 1);
                meses.Add((fatura.Month, fatura.Year));
            }

            Assert.Equal((11, 2026), meses[0]);
            Assert.Equal((12, 2026), meses[1]);
            Assert.Equal((1, 2027), meses[2]);
        }
    }
}
