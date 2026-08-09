using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ControleFinanceiro.Models.Enums;

namespace ControleFinanceiro.Models
{
    public class Transacao
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "A descrição é obrigatória.")]
        [StringLength(100, ErrorMessage = "A descrição pode ter no máximo 100 caracteres.")]
        public string Descricao { get; set; } = string.Empty;

        [Required(ErrorMessage = "O valor é obrigatório.")]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, 1000000.00, ErrorMessage = "O valor deve ser maior que zero.")]
        public decimal Valor { get; set; }

        [Required(ErrorMessage = "A data é obrigatória.")]
        [DataType(DataType.Date)]
        public DateTime Data { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "O tipo é obrigatório.")]
        public TipoTransacao Tipo { get; set; }

        [Required(ErrorMessage = "A categoria é obrigatória.")]
        public int CategoriaId { get; set; }

        public Categoria? Categoria { get; set; }

        [StringLength(250, ErrorMessage = "A observação pode ter no máximo 250 caracteres.")]
        public string? Observacao { get; set; }
    }
}
