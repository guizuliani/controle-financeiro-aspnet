using ControleFinanceiro.Domain.Entities;

namespace ControleFinanceiro.Models.ViewModels
{
    public class FaturaViewModel
    {
        public CartaoCredito Cartao { get; set; } = null!;
        public int Mes { get; set; }
        public int Ano { get; set; }
        public List<Transacao> Transacoes { get; set; } = new();
        public decimal TotalFatura => Transacoes.Sum(t => t.Valor);
        public decimal TotalPontos => Transacoes.Sum(t => t.PontosGerados);
        public bool FaturaPaga => Transacoes.Any() && Transacoes.All(t => t.FaturaPaga);
        public DateTime DataFechamento => new DateTime(Ano, Mes, Math.Min(Cartao.DiaFechamento, DateTime.DaysInMonth(Ano, Mes)));
        public DateTime DataVencimento => new DateTime(Ano, Mes, Math.Min(Cartao.DiaVencimento, DateTime.DaysInMonth(Ano, Mes)));
    }
}
