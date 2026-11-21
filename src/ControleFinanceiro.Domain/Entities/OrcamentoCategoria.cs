using System.ComponentModel.DataAnnotations;

namespace ControleFinanceiro.Domain.Entities;

public class OrcamentoCategoria
{
    public int Id { get; set; }
    public int UsuarioId { get; set; } = 1;
    public Usuario? Usuario { get; set; }

    [Required(ErrorMessage = "A categoria é obrigatória.")]
    public int CategoriaId { get; set; }
    public Categoria? Categoria { get; set; }

    [Required(ErrorMessage = "O valor limite é obrigatório.")]
    [Range(0.01, 10000000.00, ErrorMessage = "O limite deve ser maior que zero.")]
    [Display(Name = "Limite Mensal (R$)")]
    public decimal ValorLimite { get; set; }

    [Range(1, 12)]
    public int Mes { get; set; }

    [Range(2000, 2100)]
    public int Ano { get; set; }

    public decimal CalcularPercentualUso(decimal totalGasto)
    {
        if (ValorLimite <= 0) return 0m;
        return Math.Round((totalGasto / ValorLimite) * 100m, 1);
    }

    public string ObterNivelAlerta(decimal totalGasto)
    {
        var pct = CalcularPercentualUso(totalGasto);
        if (pct >= 100m) return "Excedido";
        if (pct >= 80m) return "Atencao";
        return "Normal";
    }
}
