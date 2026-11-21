using System.ComponentModel.DataAnnotations;
using ControleFinanceiro.Domain.Enums;

namespace ControleFinanceiro.Domain.Entities;

public class CartaoCredito
{
    public int Id { get; set; }

    [Required(ErrorMessage = "O nome do cartão é obrigatório.")]
    [StringLength(50, ErrorMessage = "O nome pode ter no máximo 50 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    [StringLength(30)]
    public string Bandeira { get; set; } = "Mastercard";

    [StringLength(20)]
    public string CorHex { get; set; } = "#1e293b";

    [Required(ErrorMessage = "O limite do cartão é obrigatório.")]
    [Range(0.01, 10000000.00, ErrorMessage = "O limite deve ser maior que zero.")]
    public decimal LimiteTotal { get; set; }

    [Required(ErrorMessage = "O dia de fechamento é obrigatório.")]
    [Range(1, 31, ErrorMessage = "O dia de fechamento deve ser entre 1 e 31.")]
    [Display(Name = "Dia de Fechamento")]
    public int DiaFechamento { get; set; } = 1;

    [Required(ErrorMessage = "O dia de vencimento é obrigatório.")]
    [Range(1, 31, ErrorMessage = "O dia de vencimento deve ser entre 1 e 31.")]
    [Display(Name = "Dia de Vencimento")]
    public int DiaVencimento { get; set; } = 10;

    // --- Configurações de Pontos e Milhas ---
    [Display(Name = "Acumula Pontos?")]
    public bool AcumulaPontos { get; set; } = false;

    [StringLength(50)]
    [Display(Name = "Nome do Programa de Pontos")]
    public string? NomeProgramaPontos { get; set; } = "Livelo";

    [Display(Name = "Tipo de Regra")]
    public TipoRegraPontos TipoRegra { get; set; } = TipoRegraPontos.PorDolar;

    [Range(0.00, 1000.00, ErrorMessage = "O multiplicador deve ser positivo.")]
    [Display(Name = "Fator / Multiplicador de Pontos")]
    public decimal FatorPontuacao { get; set; } = 1.0m;

    [Range(0.01, 100.00, ErrorMessage = "A cotação do dólar deve ser maior que zero.")]
    [Display(Name = "Cotação do Dólar de Referência (R$)")]
    public decimal CotacaoDolarReferencia { get; set; } = 5.50m;

    public ICollection<Transacao> Transacoes { get; set; } = new List<Transacao>();
    public ICollection<ResgatePontos> Resgates { get; set; } = new List<ResgatePontos>();

    /// <summary>
    /// Calcula os pontos ou cashback gerados para um determinado valor de compra.
    /// </summary>
    public decimal CalcularPontos(decimal valor)
    {
        if (!AcumulaPontos || FatorPontuacao <= 0 || valor <= 0)
            return 0m;

        return TipoRegra switch
        {
            TipoRegraPontos.PorDolar => Math.Round((valor / (CotacaoDolarReferencia > 0 ? CotacaoDolarReferencia : 5.50m)) * FatorPontuacao, 2),
            TipoRegraPontos.PorReal => Math.Round(valor * FatorPontuacao, 2),
            TipoRegraPontos.Cashback => Math.Round(valor * (FatorPontuacao / 100m), 2),
            _ => 0m
        };
    }

    /// <summary>
    /// Determina a data de referência (mês/ano) da primeira fatura com base no dia de fechamento ("Melhor dia de compra").
    /// </summary>
    public DateTime CalcularMesInicialFatura(DateTime dataCompra)
    {
        var baseMes = new DateTime(dataCompra.Year, dataCompra.Month, 1);
        return dataCompra.Day >= DiaFechamento ? baseMes.AddMonths(1) : baseMes;
    }

    /// <summary>
    /// Calcula o limite total comprometido com lançamentos ainda não quitados.
    /// </summary>
    public decimal CalcularLimiteUtilizado() =>
        Transacoes.Where(t => !t.FaturaPaga).Sum(t => t.Valor);

    /// <summary>
    /// Calcula o limite disponível restante do cartão.
    /// </summary>
    public decimal CalcularLimiteDisponivel() =>
        Math.Max(0m, LimiteTotal - CalcularLimiteUtilizado());

    /// <summary>
    /// Calcula o saldo líquido de pontos acumulados menos resgatados.
    /// </summary>
    public decimal CalcularSaldoPontos() =>
        Transacoes.Sum(t => t.PontosGerados) - Resgates.Sum(r => r.Pontos);

    /// <summary>
    /// Retorna a descrição legível da regra de pontuação configurada.
    /// </summary>
    public string ObterDescricaoRegraPontos() => TipoRegra switch
    {
        TipoRegraPontos.PorDolar => $"{FatorPontuacao:N1} pts / US$ 1 (Dólar R$ {CotacaoDolarReferencia:N2})",
        TipoRegraPontos.PorReal => $"{FatorPontuacao:N1} pts / R$ 1 gasto",
        TipoRegraPontos.Cashback => $"{FatorPontuacao:N1}% de Cashback",
        _ => "Configurado"
    };
}
