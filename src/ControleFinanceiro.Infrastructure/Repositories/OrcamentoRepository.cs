using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Ports;
using ControleFinanceiro.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ControleFinanceiro.Infrastructure.Repositories;

public class OrcamentoRepository : IOrcamentoRepository
{
    private readonly AppDbContext _context;

    public OrcamentoRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<List<OrcamentoCategoria>> ListarPorMesAsync(int usuarioId, int mes, int ano) =>
        _context.Orcamentos
            .Include(o => o.Categoria)
            .Where(o => o.UsuarioId == usuarioId && o.Mes == mes && o.Ano == ano)
            .OrderBy(o => o.Categoria!.Nome)
            .ToListAsync();

    public Task<OrcamentoCategoria?> ObterPorIdAsync(int id, int usuarioId) =>
        _context.Orcamentos
            .Include(o => o.Categoria)
            .FirstOrDefaultAsync(o => o.Id == id && o.UsuarioId == usuarioId);

    public Task<OrcamentoCategoria?> ObterPorCategoriaEMesAsync(int usuarioId, int categoriaId, int mes, int ano) =>
        _context.Orcamentos
            .FirstOrDefaultAsync(o => o.UsuarioId == usuarioId && o.CategoriaId == categoriaId && o.Mes == mes && o.Ano == ano);

    public async Task AdicionarAsync(OrcamentoCategoria orcamento) =>
        await _context.Orcamentos.AddAsync(orcamento);

    public void Atualizar(OrcamentoCategoria orcamento) =>
        _context.Orcamentos.Update(orcamento);

    public void Remover(OrcamentoCategoria orcamento) =>
        _context.Orcamentos.Remove(orcamento);
}
