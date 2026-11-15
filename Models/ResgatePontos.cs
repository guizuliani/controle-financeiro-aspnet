using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ControleFinanceiro.Models
{
    public class ResgatePontos
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O cartão é obrigatório.")]
        public int CartaoCreditoId { get; set; }

        public CartaoCredito? CartaoCredito { get; set; }

        [Required(ErrorMessage = "A data é obrigatória.")]
        [DataType(DataType.Date)]
        public DateTime Data { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "A quantidade de pontos é obrigatória.")]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, 100000000.00, ErrorMessage = "A quantidade de pontos deve ser maior que zero.")]
        public decimal Pontos { get; set; }

        [Required(ErrorMessage = "A descrição do resgate/transferência é obrigatória.")]
        [StringLength(100, ErrorMessage = "A descrição pode ter no máximo 100 caracteres.")]
        public string Descricao { get; set; } = string.Empty;
    }
}
