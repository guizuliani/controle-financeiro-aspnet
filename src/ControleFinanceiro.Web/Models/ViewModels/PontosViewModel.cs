using ControleFinanceiro.Domain.Entities;

namespace ControleFinanceiro.Models.ViewModels
{
    public class PontosViewModel
    {
        public List<ProgramaPontosItem> Programas { get; set; } = new();
        public List<Transacao> ExtratoTransacoes { get; set; } = new();
        public List<ResgatePontos> HistoricoResgates { get; set; } = new();

        public decimal SaldoTotalGeral => Programas.Sum(p => p.SaldoAtual);
    }

    public class ProgramaPontosItem
    {
        public string NomePrograma { get; set; } = string.Empty;
        public string CartaoNome { get; set; } = string.Empty;
        public int CartaoId { get; set; }
        public string CorHex { get; set; } = "#1e293b";
        public decimal TotalAcumulado { get; set; }
        public decimal TotalResgatado { get; set; }
        public decimal SaldoAtual => Math.Max(0, TotalAcumulado - TotalResgatado);
        public string RegraDescricao { get; set; } = string.Empty;
    }
}
