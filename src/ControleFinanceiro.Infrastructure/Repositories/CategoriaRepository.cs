using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Ports;
using ControleFinanceiro.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ControleFinanceiro.Infrastructure.Repositories;

public class CategoriaRepository : ICategoriaRepository
{
    private readonly AppDbContext _context;

    public CategoriaRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<List<Categoria>> ListarTodasOrdenadasPorNomeAsync(bool incluirTransacoes = false, int usuarioId = 1)
    {
        var query = _context.Categorias.Where(c => c.UsuarioId == usuarioId);

        if (incluirTransacoes)
            query = query.Include(c => c.Transacoes);

        return query.OrderBy(c => c.Nome).ToListAsync();
    }

    public Task<Categoria?> ObterPorIdAsync(int id, bool incluirTransacoes = false, int usuarioId = 1)
    {
        var query = _context.Categorias.Where(c => c.UsuarioId == usuarioId);

        if (incluirTransacoes)
            query = query.Include(c => c.Transacoes);

        return query.FirstOrDefaultAsync(c => c.Id == id);
    }

    public Task<Categoria?> ObterCategoriaPagamentoFaturaAsync(int usuarioId = 1) =>
        _context.Categorias
            .Where(c => c.UsuarioId == usuarioId)
            .FirstOrDefaultAsync(c => c.Nome.Contains("Moradia") || c.Nome.Contains("Outros"))
            .ContinueWith(async t => await t ?? await _context.Categorias.FirstOrDefaultAsync(c => c.UsuarioId == usuarioId))
            .Unwrap();

    public Task<bool> ExisteAsync(int id, int usuarioId = 1) =>
        _context.Categorias.AnyAsync(c => c.Id == id && c.UsuarioId == usuarioId);

    public async Task AdicionarAsync(Categoria categoria) =>
        await _context.Categorias.AddAsync(categoria);

    public async Task AdicionarVariasAsync(IEnumerable<Categoria> categorias) =>
        await _context.Categorias.AddRangeAsync(categorias);

    public void Atualizar(Categoria categoria) =>
        _context.Categorias.Update(categoria);

    public void Remover(Categoria categoria) =>
        _context.Categorias.Remove(categoria);
}
