namespace ControleFinanceiro.Models.ViewModels
{
    public class DashboardViewModel
    {
        public decimal TotalReceitas { get; set; }
        public decimal TotalDespesas { get; set; }
        public decimal SaldoTotal => TotalReceitas - TotalDespesas;

        public int MesAtual { get; set; }
        public int AnoAtual { get; set; }

        public List<Transacao> UltimasTransacoes { get; set; } = new();
        public List<GraficoCategoriaItem> DespesasPorCategoria { get; set; } = new();
    }

    public class GraficoCategoriaItem
    {
        public string Categoria { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public string CorHex { get; set; } = "#6c757d";
    }
}
