using System.ComponentModel.DataAnnotations;

namespace ControleFinanceiro.Models
{
    public class Categoria
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome da categoria é obrigatório.")]
        [StringLength(50, ErrorMessage = "O nome pode ter no máximo 50 caracteres.")]
        public string Nome { get; set; } = string.Empty;

        [StringLength(20)]
        public string CorHex { get; set; } = "#6c757d";

        [StringLength(50)]
        public string Icone { get; set; } = "bi-tag";

        public ICollection<Transacao> Transacoes { get; set; } = new List<Transacao>();
    }
}
