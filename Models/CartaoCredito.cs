using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ControleFinanceiro.Models.Enums;

namespace ControleFinanceiro.Models
{
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
        [Column(TypeName = "decimal(18,2)")]
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

        [Column(TypeName = "decimal(18,2)")]
        [Range(0.00, 1000.00, ErrorMessage = "O multiplicador deve ser positivo.")]
        [Display(Name = "Fator / Multiplicador de Pontos")]
        public decimal FatorPontuacao { get; set; } = 1.0m;

        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, 100.00, ErrorMessage = "A cotação do dólar deve ser maior que zero.")]
        [Display(Name = "Cotação do Dólar de Referência (R$)")]
        public decimal CotacaoDolarReferencia { get; set; } = 5.50m;

        public ICollection<Transacao> Transacoes { get; set; } = new List<Transacao>();
        public ICollection<ResgatePontos> Resgates { get; set; } = new List<ResgatePontos>();
    }
}
