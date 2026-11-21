using System.ComponentModel.DataAnnotations;
using ControleFinanceiro.Domain.Enums;

namespace ControleFinanceiro.Domain.Entities;

public class LancamentoRecorrente
{
    public int Id { get; set; }
    public int UsuarioId { get; set; } = 1;
    public Usuario? Usuario { get; set; }

    [Required(ErrorMessage = "A descrição é obrigatória.")]
    [StringLength(100)]
    public string Descricao { get; set; } = string.Empty;

    [Required(ErrorMessage = "O valor é obrigatório.")]
    [Range(0.01, 1000000.00, ErrorMessage = "O valor deve ser maior que zero.")]
    public decimal Valor { get; set; }

    public TipoTransacao Tipo { get; set; } = TipoTransacao.Despesa;

    [Required(ErrorMessage = "A categoria é obrigatória.")]
    public int CategoriaId { get; set; }
    public Categoria? Categoria { get; set; }

    [Display(Name = "Forma de Pagamento")]
    public FormaPagamento FormaPagamento { get; set; } = FormaPagamento.ContaCorrente;

    [Display(Name = "Conta Bancária")]
    public int? ContaBancariaId { get; set; }
    public ContaBancaria? ContaBancaria { get; set; }

    [Display(Name = "Cartão de Crédito")]
    public int? CartaoCreditoId { get; set; }
    public CartaoCredito? CartaoCredito { get; set; }

    [Range(1, 31, ErrorMessage = "O dia deve estar entre 1 e 31.")]
    [Display(Name = "Dia do Mês")]
    public int DiaDoMes { get; set; } = 5;

    public bool Ativo { get; set; } = true;
    public int? UltimoMesGerado { get; set; }
    public int? UltimoAnoGerado { get; set; }

    public bool JaGeradoParaMes(int mes, int ano) =>
        UltimoMesGerado == mes && UltimoAnoGerado == ano;

    public DateTime CalcularDataCompetencia(int mes, int ano)
    {
        int dia = Math.Min(DiaDoMes, DateTime.DaysInMonth(ano, mes));
        return new DateTime(ano, mes, dia);
    }
}
