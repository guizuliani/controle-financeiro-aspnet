using System.ComponentModel.DataAnnotations;
using ControleFinanceiro.Domain.Enums;

namespace ControleFinanceiro.Domain.Entities;

public class Transacao
{
    public int Id { get; set; }
    public int UsuarioId { get; set; } = 1;
    public Usuario? Usuario { get; set; }

    [Required(ErrorMessage = "A descrição é obrigatória.")]
    [StringLength(100, ErrorMessage = "A descrição pode ter no máximo 100 caracteres.")]
    public string Descricao { get; set; } = string.Empty;

    [Required(ErrorMessage = "O valor é obrigatório.")]
    [Range(0.01, 1000000.00, ErrorMessage = "O valor deve ser maior que zero.")]
    public decimal Valor { get; set; }

    [Required(ErrorMessage = "A data é obrigatória.")]
    [DataType(DataType.Date)]
    public DateTime Data { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "O tipo é obrigatório.")]
    public TipoTransacao Tipo { get; set; } = TipoTransacao.Despesa;

    [Required(ErrorMessage = "A categoria é obrigatória.")]
    public int CategoriaId { get; set; }

    public Categoria? Categoria { get; set; }

    [StringLength(250, ErrorMessage = "A observação pode ter no máximo 250 caracteres.")]
    public string? Observacao { get; set; }

    // --- Cartão de Crédito e Parcelamento ---
    [Display(Name = "Forma de Pagamento")]
    public FormaPagamento FormaPagamento { get; set; } = FormaPagamento.ContaCorrente;

    [Display(Name = "Conta Bancária")]
    public int? ContaBancariaId { get; set; }

    public ContaBancaria? ContaBancaria { get; set; }

    [Display(Name = "Cartão de Crédito")]
    public int? CartaoCreditoId { get; set; }

    public CartaoCredito? CartaoCredito { get; set; }

    [Display(Name = "Nº Parcela")]
    public int NumeroParcela { get; set; } = 1;

    [Display(Name = "Total de Parcelas")]
    public int TotalParcelas { get; set; } = 1;

    public Guid? CompraId { get; set; }

    [Display(Name = "Mês da Fatura")]
    public int? FaturaMes { get; set; }

    [Display(Name = "Ano da Fatura")]
    public int? FaturaAno { get; set; }

    [Display(Name = "Pontos Gerados")]
    public decimal PontosGerados { get; set; } = 0;

    [Display(Name = "Fatura Paga")]
    public bool FaturaPaga { get; set; } = false;

    /// <summary>
    /// Gera a lista de transações (parcelas) para uma compra no cartão de crédito,
    /// distribuindo eventual diferença de centavos na última parcela.
    /// </summary>
    public static IReadOnlyList<Transacao> GerarParcelasCartao(Transacao compraBase, CartaoCredito cartao)
    {
        int parcelas = Math.Max(1, compraBase.TotalParcelas);
        decimal valorParcela = Math.Round(compraBase.Valor / parcelas, 2);
        decimal diferencaCentavos = compraBase.Valor - (valorParcela * parcelas);
        Guid compraId = Guid.NewGuid();

        DateTime mesInicialFatura = cartao.CalcularMesInicialFatura(compraBase.Data);
        var resultado = new List<Transacao>(parcelas);

        for (int i = 1; i <= parcelas; i++)
        {
            var mesFaturaData = mesInicialFatura.AddMonths(i - 1);
            decimal valorAtual = (i == parcelas) ? valorParcela + diferencaCentavos : valorParcela;
            decimal pontos = cartao.CalcularPontos(valorAtual);

            resultado.Add(new Transacao
            {
                UsuarioId = compraBase.UsuarioId,
                Descricao = parcelas > 1 ? $"{compraBase.Descricao} ({i}/{parcelas})" : compraBase.Descricao,
                Valor = valorAtual,
                Data = compraBase.Data,
                Tipo = TipoTransacao.Despesa, // Compra no cartão é sempre despesa
                CategoriaId = compraBase.CategoriaId,
                Observacao = compraBase.Observacao,
                FormaPagamento = FormaPagamento.CartaoCredito,
                CartaoCreditoId = cartao.Id,
                NumeroParcela = i,
                TotalParcelas = parcelas,
                CompraId = compraId,
                FaturaMes = mesFaturaData.Month,
                FaturaAno = mesFaturaData.Year,
                PontosGerados = pontos,
                FaturaPaga = false
            });
        }

        return resultado;
    }

    /// <summary>
    /// Normaliza os campos de uma transação à vista em Conta Corrente.
    /// </summary>
    public void PrepararTransacaoContaCorrente()
    {
        FormaPagamento = FormaPagamento.ContaCorrente;
        NumeroParcela = 1;
        TotalParcelas = 1;
        PontosGerados = 0;
        CartaoCreditoId = null;
        CompraId = null;
        FaturaMes = null;
        FaturaAno = null;
        FaturaPaga = false;
    }
}
