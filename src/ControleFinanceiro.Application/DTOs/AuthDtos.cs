namespace ControleFinanceiro.Application.DTOs;

public record UsuarioAutenticadoDto(int Id, string Nome, string Email);

public record ResultadoAuth(bool Sucesso, string Mensagem, UsuarioAutenticadoDto? Usuario = null)
{
    public static ResultadoAuth Ok(UsuarioAutenticadoDto usuario, string mensagem = "Autenticação realizada com sucesso!") =>
        new(true, mensagem, usuario);

    public static ResultadoAuth Falha(string mensagem) =>
        new(false, mensagem, null);
}
