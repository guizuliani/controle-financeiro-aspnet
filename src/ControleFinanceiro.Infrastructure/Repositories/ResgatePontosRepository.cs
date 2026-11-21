using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Ports;
using ControleFinanceiro.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ControleFinanceiro.Infrastructure.Repositories;

public class ResgatePontosRepository : IResgatePontosRepository
{
    private readonly AppDbContext _context;

    public ResgatePontosRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<List<ResgatePontos>> ListarUltimosAsync(int quantidade, int usuarioId = 1) =>
        _context.ResgatesPontos
            .Include(r => r.CartaoCredito)
            .Where(r => r.UsuarioId == usuarioId)
            .OrderByDescending(r => r.Data)
            .Take(quantidade)
            .ToListAsync();

    public Task<decimal> SomarTotalPontosResgatadosAsync(int usuarioId = 1) =>
        _context.ResgatesPontos.Where(r => r.UsuarioId == usuarioId).SumAsync(r => r.Pontos);

    public async Task AdicionarAsync(ResgatePontos resgate) =>
        await _context.ResgatesPontos.AddAsync(resgate);
}
