using System.ComponentModel.DataAnnotations;
using ControleFinanceiro.Domain.Enums;

namespace ControleFinanceiro.Domain.Entities;

public class ContaBancaria
{
    public int Id { get; set; }
    public int UsuarioId { get; set; } = 1;
    public Usuario? Usuario { get; set; }

    [Required(ErrorMessage = "O nome da conta é obrigatório.")]
    [StringLength(60, ErrorMessage = "O nome pode ter no máximo 60 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    [StringLength(50)]
    [Display(Name = "Instituição / Banco")]
    public string Instituicao { get; set; } = string.Empty;

    [Display(Name = "Tipo de Conta")]
    public TipoConta Tipo { get; set; } = TipoConta.ContaCorrente;

    [Display(Name = "Saldo Inicial")]
    public decimal SaldoInicial { get; set; } = 0m;

    [StringLength(20)]
    public string CorHex { get; set; } = "#2563eb";

    public ICollection<Transacao> Transacoes { get; set; } = new List<Transacao>();

    public decimal CalcularSaldoAtual()
    {
        var movimentacoes = Transacoes
            .Where(t => t.FormaPagamento == FormaPagamento.ContaCorrente);

        var receitas = movimentacoes.Where(t => t.Tipo == TipoTransacao.Receita).Sum(t => t.Valor);
        var despesas = movimentacoes.Where(t => t.Tipo == TipoTransacao.Despesa).Sum(t => t.Valor);

        return SaldoInicial + receitas - despesas;
    }
}
