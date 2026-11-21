using ControleFinanceiro.Domain.Enums;

namespace ControleFinanceiro.Application.DTOs;

public class ContaBancariaResumoDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Instituicao { get; set; } = string.Empty;
    public TipoConta Tipo { get; set; }
    public decimal SaldoInicial { get; set; }
    public decimal SaldoAtual { get; set; }
    public string CorHex { get; set; } = "#2563eb";
}

public class OrcamentoResumoDto
{
    public int Id { get; set; }
    public int CategoriaId { get; set; }
    public string CategoriaNome { get; set; } = string.Empty;
    public string CategoriaCorHex { get; set; } = "#6c757d";
    public decimal ValorLimite { get; set; }
    public decimal ValorGasto { get; set; }
    public decimal PercentualUso { get; set; }
    public string NivelAlerta { get; set; } = "Normal";
    public int Mes { get; set; }
    public int Ano { get; set; }
}

public class ItemImportacaoDto
{
    public DateTime Data { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public TipoTransacao Tipo { get; set; }
}
