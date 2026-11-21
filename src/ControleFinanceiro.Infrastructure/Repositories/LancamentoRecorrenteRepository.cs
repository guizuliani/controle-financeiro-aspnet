using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Ports;
using ControleFinanceiro.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ControleFinanceiro.Infrastructure.Repositories;

public class LancamentoRecorrenteRepository : ILancamentoRecorrenteRepository
{
    private readonly AppDbContext _context;

    public LancamentoRecorrenteRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<List<LancamentoRecorrente>> ListarPorUsuarioAsync(int usuarioId) =>
        _context.LancamentosRecorrentes
            .Include(l => l.Categoria)
            .Include(l => l.ContaBancaria)
            .Include(l => l.CartaoCredito)
            .Where(l => l.UsuarioId == usuarioId)
            .OrderBy(l => l.DiaDoMes)
            .ToListAsync();

    public Task<List<LancamentoRecorrente>> ListarAtivosPorUsuarioAsync(int usuarioId) =>
        _context.LancamentosRecorrentes
            .Where(l => l.UsuarioId == usuarioId && l.Ativo)
            .ToListAsync();

    public Task<LancamentoRecorrente?> ObterPorIdAsync(int id, int usuarioId) =>
        _context.LancamentosRecorrentes
            .FirstOrDefaultAsync(l => l.Id == id && l.UsuarioId == usuarioId);

    public async Task AdicionarAsync(LancamentoRecorrente recorrente) =>
        await _context.LancamentosRecorrentes.AddAsync(recorrente);

    public void Atualizar(LancamentoRecorrente recorrente) =>
        _context.LancamentosRecorrentes.Update(recorrente);

    public void Remover(LancamentoRecorrente recorrente) =>
        _context.LancamentosRecorrentes.Remove(recorrente);
}
