using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ControleFinanceiro.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Categoria> Categorias { get; set; } = null!;
    public DbSet<Transacao> Transacoes { get; set; } = null!;
    public DbSet<CartaoCredito> CartoesCredito { get; set; } = null!;
    public DbSet<ResgatePontos> ResgatesPontos { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<CartaoCredito>(entity =>
        {
            entity.Property(e => e.LimiteTotal).HasColumnType("decimal(18,2)");
            entity.Property(e => e.FatorPontuacao).HasColumnType("decimal(18,2)");
            entity.Property(e => e.CotacaoDolarReferencia).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<Transacao>(entity =>
        {
            entity.Property(e => e.Valor).HasColumnType("decimal(18,2)");
            entity.Property(e => e.PontosGerados).HasColumnType("decimal(18,2)");

            entity.HasOne(t => t.Categoria)
                .WithMany(c => c.Transacoes)
                .HasForeignKey(t => t.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(t => t.CartaoCredito)
                .WithMany(c => c.Transacoes)
                .HasForeignKey(t => t.CartaoCreditoId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ResgatePontos>(entity =>
        {
            entity.Property(e => e.Pontos).HasColumnType("decimal(18,2)");

            entity.HasOne(r => r.CartaoCredito)
                .WithMany(c => c.Resgates)
                .HasForeignKey(r => r.CartaoCreditoId)
                .OnDelete(DeleteBehavior.Cascade);
        });

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

        modelBuilder.Entity<CartaoCredito>().HasData(
            new CartaoCredito
            {
                Id = 1,
                Nome = "C6 Carbon Mastercard Black",
                Bandeira = "Mastercard",
                CorHex = "#1e293b",
                LimiteTotal = 15000.00m,
                DiaFechamento = 20,
                DiaVencimento = 28,
                AcumulaPontos = true,
                NomeProgramaPontos = "C6 Átomos",
                TipoRegra = TipoRegraPontos.PorDolar,
                FatorPontuacao = 2.5m,
                CotacaoDolarReferencia = 5.50m
            },
            new CartaoCredito
            {
                Id = 2,
                Nome = "Nubank Ultravioleta",
                Bandeira = "Mastercard",
                CorHex = "#820ad1",
                LimiteTotal = 8000.00m,
                DiaFechamento = 10,
                DiaVencimento = 17,
                AcumulaPontos = true,
                NomeProgramaPontos = "Cashback Nu",
                TipoRegra = TipoRegraPontos.Cashback,
                FatorPontuacao = 1.0m,
                CotacaoDolarReferencia = 5.50m
            }
        );
    }
}
