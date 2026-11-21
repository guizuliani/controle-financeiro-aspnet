namespace ControleFinanceiro.Application.DTOs;

public record ResultadoOperacao(bool Sucesso, string Mensagem, string? CampoErro = null)
{
    public static ResultadoOperacao Ok(string mensagem) => new(true, mensagem);
    public static ResultadoOperacao Falha(string mensagem, string? campoErro = null) => new(false, mensagem, campoErro);
}
