using ControleFinanceiro.Domain.Entities;

namespace ControleFinanceiro.Application.DTOs;

public class FaturaDto
{
    public CartaoCredito Cartao { get; set; } = null!;
    public int Mes { get; set; }
    public int Ano { get; set; }
    public List<Transacao> Transacoes { get; set; } = new();
}

public class PontosPainelDto
{
    public List<ProgramaPontosDto> Programas { get; set; } = new();
    public List<Transacao> ExtratoTransacoes { get; set; } = new();
    public List<ResgatePontos> HistoricoResgates { get; set; } = new();
}

public class ProgramaPontosDto
{
    public string NomePrograma { get; set; } = string.Empty;
    public string CartaoNome { get; set; } = string.Empty;
    public int CartaoId { get; set; }
    public string CorHex { get; set; } = "#1e293b";
    public decimal TotalAcumulado { get; set; }
    public decimal TotalResgatado { get; set; }
    public string RegraDescricao { get; set; } = string.Empty;
}
