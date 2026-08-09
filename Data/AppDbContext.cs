using Microsoft.EntityFrameworkCore;
using ControleFinanceiro.Models;

namespace ControleFinanceiro.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Categoria> Categorias { get; set; } = null!;
        public DbSet<Transacao> Transacoes { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Categoria>().HasData(
                new Categoria { Id = 1, Nome = "Salário & Rendas", CorHex = "#198754", Icone = "bi-wallet2" },
                new Categoria { Id = 2, Nome = "Alimentação", CorHex = "#fd7e14", Icone = "bi-cart4" },
                new Categoria { Id = 3, Nome = "Moradia & Contas", CorHex = "#0d6efd", Icone = "bi-house-door" },
                new Categoria { Id = 4, Nome = "Transporte", CorHex = "#6f42c1", Icone = "bi-fuel-pump" },
                new Categoria { Id = 5, Nome = "Lazer & Viagens", CorHex = "#e83e8c", Icone = "bi-controller" },
                new Categoria { Id = 6, Nome = "Saúde & Cuidados", CorHex = "#dc3545", Icone = "bi-heart-pulse" },
                new Categoria { Id = 7, Nome = "Investimentos", CorHex = "#20c997", Icone = "bi-graph-up-arrow" },
                new Categoria { Id = 8, Nome = "Outros", CorHex = "#6c757d", Icone = "bi-three-dots" }
            );

            modelBuilder.Entity<Transacao>()
                .HasOne(t => t.Categoria)
                .WithMany(c => c.Transacoes)
                .HasForeignKey(t => t.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
