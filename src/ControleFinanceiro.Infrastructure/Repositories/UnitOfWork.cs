using ControleFinanceiro.Domain.Ports;
using ControleFinanceiro.Infrastructure.Persistence;

namespace ControleFinanceiro.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public Task<int> SalvarAlteracoesAsync() =>
        _context.SaveChangesAsync();
}
