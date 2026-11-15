namespace ControleFinanceiro.Models.ViewModels
{
    public class DashboardViewModel
    {
        public decimal TotalReceitas { get; set; }
        public decimal TotalDespesas { get; set; }
        public decimal TotalFaturasMes { get; set; }
        public decimal SaldoTotal => TotalReceitas - TotalDespesas;

        public decimal TotalPontosAcumulados { get; set; }

        public int MesAtual { get; set; }
        public int AnoAtual { get; set; }

        public List<Transacao> UltimasTransacoes { get; set; } = new();
        public List<GraficoCategoriaItem> DespesasPorCategoria { get; set; } = new();
        public List<CartaoResumoItem> CartoesResumo { get; set; } = new();
    }

    public class GraficoCategoriaItem
    {
        public string Categoria { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public string CorHex { get; set; } = "#6c757d";
    }

    public class CartaoResumoItem
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string CorHex { get; set; } = "#1e293b";
        public decimal LimiteTotal { get; set; }
        public decimal LimiteUtilizado { get; set; }
        public decimal LimiteDisponivel => Math.Max(0, LimiteTotal - LimiteUtilizado);
        public decimal PercentualUso => LimiteTotal > 0 ? Math.Min(100, Math.Round((LimiteUtilizado / LimiteTotal) * 100, 1)) : 0;
        public decimal FaturaMesAtual { get; set; }
        public int DiaFechamento { get; set; }
        public int DiaVencimento { get; set; }
        public string NomeProgramaPontos { get; set; } = string.Empty;
    }
}
