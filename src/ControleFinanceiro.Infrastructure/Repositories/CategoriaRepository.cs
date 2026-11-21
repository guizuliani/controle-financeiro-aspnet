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

    public Task<List<Categoria>> ListarTodasOrdenadasPorNomeAsync(bool incluirTransacoes = false)
    {
        var query = _context.Categorias.AsQueryable();

        if (incluirTransacoes)
            query = query.Include(c => c.Transacoes);

        return query.OrderBy(c => c.Nome).ToListAsync();
    }

    public Task<Categoria?> ObterPorIdAsync(int id, bool incluirTransacoes = false)
    {
        var query = _context.Categorias.AsQueryable();

        if (incluirTransacoes)
            query = query.Include(c => c.Transacoes);

        return query.FirstOrDefaultAsync(c => c.Id == id);
    }

    public Task<Categoria?> ObterCategoriaPagamentoFaturaAsync() =>
        _context.Categorias.FirstOrDefaultAsync(c => c.Nome.Contains("Moradia") || c.Nome.Contains("Outros"));

    public Task<bool> ExisteAsync(int id) =>
        _context.Categorias.AnyAsync(c => c.Id == id);

    public async Task AdicionarAsync(Categoria categoria) =>
        await _context.Categorias.AddAsync(categoria);

    public void Atualizar(Categoria categoria) =>
        _context.Categorias.Update(categoria);

    public void Remover(Categoria categoria) =>
        _context.Categorias.Remove(categoria);
}
