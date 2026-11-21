using ControleFinanceiro.Domain.Entities;

namespace ControleFinanceiro.Domain.Ports;

public interface IContaBancariaRepository
{
    Task<List<ContaBancaria>> ListarPorUsuarioAsync(int usuarioId, bool incluirTransacoes = false);
    Task<ContaBancaria?> ObterPorIdAsync(int id, int usuarioId, bool incluirTransacoes = false);
    Task AdicionarAsync(ContaBancaria conta);
    void Atualizar(ContaBancaria conta);
    void Remover(ContaBancaria conta);
}
