using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Domain.Entities;

namespace ControleFinanceiro.Application.Ports;

public interface ICategoriaService
{
    Task<List<Categoria>> ListarTodasAsync(bool incluirTransacoes = false, int usuarioId = 1);
    Task<Categoria?> ObterPorIdAsync(int id, bool incluirTransacoes = false, int usuarioId = 1);
    Task<ResultadoOperacao> CriarAsync(Categoria categoria);
    Task<ResultadoOperacao> AtualizarAsync(Categoria categoria);
    Task<ResultadoOperacao> ExcluirAsync(int id, int usuarioId = 1);
}
