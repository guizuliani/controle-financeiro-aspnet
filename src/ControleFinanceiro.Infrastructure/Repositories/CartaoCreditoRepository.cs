using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Ports;
using ControleFinanceiro.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ControleFinanceiro.Infrastructure.Repositories;

public class CartaoCreditoRepository : ICartaoCreditoRepository
{
    private readonly AppDbContext _context;

    public CartaoCreditoRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<List<CartaoCredito>> ListarTodosOrdenadosPorNomeAsync(
        bool incluirTransacoes = false,
        bool incluirResgates = false,
        int usuarioId = 1)
    {
        var query = _context.CartoesCredito.Where(c => c.UsuarioId == usuarioId);

        if (incluirTransacoes)
            query = query.Include(c => c.Transacoes);

        if (incluirResgates)
            query = query.Include(c => c.Resgates);

        return query.OrderBy(c => c.Nome).ToListAsync();
    }

    public Task<List<CartaoCredito>> ListarComPontosAsync(bool incluirTransacoesEResgates = false, int usuarioId = 1)
    {
        var query = _context.CartoesCredito.Where(c => c.UsuarioId == usuarioId && c.AcumulaPontos);

        if (incluirTransacoesEResgates)
        {
            query = query
                .Include(c => c.Transacoes)
                .Include(c => c.Resgates);
        }

        return query.OrderBy(c => c.Nome).ToListAsync();
    }

    public Task<CartaoCredito?> ObterPorIdAsync(
        int id,
        bool incluirTransacoes = false,
        bool incluirResgates = false,
        int usuarioId = 1)
    {
        var query = _context.CartoesCredito.Where(c => c.UsuarioId == usuarioId);

        if (incluirTransacoes)
            query = query.Include(c => c.Transacoes);

        if (incluirResgates)
            query = query.Include(c => c.Resgates);

        return query.FirstOrDefaultAsync(c => c.Id == id);
    }

    public Task<bool> ExisteAsync(int id, int usuarioId = 1) =>
        _context.CartoesCredito.AnyAsync(c => c.Id == id && c.UsuarioId == usuarioId);

    public async Task AdicionarAsync(CartaoCredito cartao) =>
        await _context.CartoesCredito.AddAsync(cartao);

    public void Atualizar(CartaoCredito cartao) =>
        _context.CartoesCredito.Update(cartao);

    public void Remover(CartaoCredito cartao) =>
        _context.CartoesCredito.Remove(cartao);
}
