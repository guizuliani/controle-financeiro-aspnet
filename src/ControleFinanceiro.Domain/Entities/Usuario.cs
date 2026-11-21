using System.ComponentModel.DataAnnotations;

namespace ControleFinanceiro.Domain.Entities;

public class Usuario
{
    public int Id { get; set; }

    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(80, ErrorMessage = "O nome pode ter no máximo 80 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    [StringLength(120)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string SenhaHash { get; set; } = string.Empty;

    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

    public ICollection<ContaBancaria> Contas { get; set; } = new List<ContaBancaria>();
    public ICollection<CartaoCredito> Cartoes { get; set; } = new List<CartaoCredito>();
    public ICollection<Categoria> Categorias { get; set; } = new List<Categoria>();
    public ICollection<Transacao> Transacoes { get; set; } = new List<Transacao>();
}
