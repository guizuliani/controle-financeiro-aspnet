using ControleFinanceiro.Application.DTOs;

namespace ControleFinanceiro.Application.Ports;

public interface IAuthService
{
    Task<ResultadoAuth> RegistrarAsync(string nome, string email, string senha);
    Task<ResultadoAuth> AutenticarAsync(string email, string senha);
}
