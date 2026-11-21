using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Ports;
using ControleFinanceiro.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ControleFinanceiro.Infrastructure.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly AppDbContext _context;

    public UsuarioRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<Usuario?> ObterPorIdAsync(int id) =>
        _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id);

    public Task<Usuario?> ObterPorEmailAsync(string email) =>
        _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);

    public Task<bool> EmailExisteAsync(string email) =>
        _context.Usuarios.AnyAsync(u => u.Email == email);

    public async Task AdicionarAsync(Usuario usuario) =>
        await _context.Usuarios.AddAsync(usuario);
}
