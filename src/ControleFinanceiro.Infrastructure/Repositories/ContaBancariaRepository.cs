using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Ports;
using ControleFinanceiro.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ControleFinanceiro.Infrastructure.Repositories;

public class ContaBancariaRepository : IContaBancariaRepository
{
    private readonly AppDbContext _context;

    public ContaBancariaRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<List<ContaBancaria>> ListarPorUsuarioAsync(int usuarioId, bool incluirTransacoes = false)
    {
        var query = _context.ContasBancarias.Where(c => c.UsuarioId == usuarioId);

        if (incluirTransacoes)
            query = query.Include(c => c.Transacoes);

        return query.OrderBy(c => c.Nome).ToListAsync();
    }

    public Task<ContaBancaria?> ObterPorIdAsync(int id, int usuarioId, bool incluirTransacoes = false)
    {
        var query = _context.ContasBancarias.Where(c => c.UsuarioId == usuarioId);

        if (incluirTransacoes)
            query = query.Include(c => c.Transacoes);

        return query.FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task AdicionarAsync(ContaBancaria conta) =>
        await _context.ContasBancarias.AddAsync(conta);

    public void Atualizar(ContaBancaria conta) =>
        _context.ContasBancarias.Update(conta);

    public void Remover(ContaBancaria conta) =>
        _context.ContasBancarias.Remove(conta);
}
