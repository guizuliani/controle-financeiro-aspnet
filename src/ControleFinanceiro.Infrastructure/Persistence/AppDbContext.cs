using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ControleFinanceiro.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Usuario> Usuarios { get; set; } = null!;
    public DbSet<ContaBancaria> ContasBancarias { get; set; } = null!;
    public DbSet<Categoria> Categorias { get; set; } = null!;
    public DbSet<Transacao> Transacoes { get; set; } = null!;
    public DbSet<CartaoCredito> CartoesCredito { get; set; } = null!;
    public DbSet<ResgatePontos> ResgatesPontos { get; set; } = null!;
    public DbSet<OrcamentoCategoria> Orcamentos { get; set; } = null!;
    public DbSet<LancamentoRecorrente> LancamentosRecorrentes { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
        });

        modelBuilder.Entity<ContaBancaria>(entity =>
        {
            entity.Property(e => e.SaldoInicial).HasColumnType("decimal(18,2)");

            entity.HasOne(c => c.Usuario)
                .WithMany(u => u.Contas)
                .HasForeignKey(c => c.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CartaoCredito>(entity =>
        {
            entity.Property(e => e.LimiteTotal).HasColumnType("decimal(18,2)");
            entity.Property(e => e.FatorPontuacao).HasColumnType("decimal(18,2)");
            entity.Property(e => e.CotacaoDolarReferencia).HasColumnType("decimal(18,2)");

            entity.HasOne(c => c.Usuario)
                .WithMany(u => u.Cartoes)
                .HasForeignKey(c => c.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.HasOne(c => c.Usuario)
                .WithMany(u => u.Categorias)
                .HasForeignKey(c => c.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Transacao>(entity =>
        {
            entity.Property(e => e.Valor).HasColumnType("decimal(18,2)");
            entity.Property(e => e.PontosGerados).HasColumnType("decimal(18,2)");

            entity.HasOne(t => t.Usuario)
                .WithMany(u => u.Transacoes)
                .HasForeignKey(t => t.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(t => t.Categoria)
                .WithMany(c => c.Transacoes)
                .HasForeignKey(t => t.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(t => t.CartaoCredito)
                .WithMany(c => c.Transacoes)
                .HasForeignKey(t => t.CartaoCreditoId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(t => t.ContaBancaria)
                .WithMany(c => c.Transacoes)
                .HasForeignKey(t => t.ContaBancariaId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ResgatePontos>(entity =>
        {
            entity.Property(e => e.Pontos).HasColumnType("decimal(18,2)");

            entity.HasOne(r => r.Usuario)
                .WithMany()
                .HasForeignKey(r => r.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(r => r.CartaoCredito)
                .WithMany(c => c.Resgates)
                .HasForeignKey(r => r.CartaoCreditoId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrcamentoCategoria>(entity =>
        {
            entity.Property(e => e.ValorLimite).HasColumnType("decimal(18,2)");
            entity.HasIndex(o => new { o.UsuarioId, o.CategoriaId, o.Mes, o.Ano }).IsUnique();

            entity.HasOne(o => o.Usuario)
                .WithMany()
                .HasForeignKey(o => o.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(o => o.Categoria)
                .WithMany()
                .HasForeignKey(o => o.CategoriaId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<LancamentoRecorrente>(entity =>
        {
            entity.Property(e => e.Valor).HasColumnType("decimal(18,2)");

            entity.HasOne(l => l.Usuario)
                .WithMany()
                .HasForeignKey(l => l.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(l => l.Categoria)
                .WithMany()
                .HasForeignKey(l => l.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(l => l.ContaBancaria)
                .WithMany()
                .HasForeignKey(l => l.ContaBancariaId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(l => l.CartaoCredito)
                .WithMany()
                .HasForeignKey(l => l.CartaoCreditoId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Usuário padrão inicial (admin@financecontrol.local / Admin@123)
        modelBuilder.Entity<Usuario>().HasData(
            new Usuario
            {
                Id = 1,
                Nome = "Administrador",
                Email = "admin@financecontrol.local",
                SenhaHash = "AQIDBAUGBwgJCgsMDQ4PEA==.7gQDaNbD2TJ9Tv/U3z+oOOw+byXCRpvOoV5EbjMrc1w=",
                CriadoEm = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );

        modelBuilder.Entity<ContaBancaria>().HasData(
            new ContaBancaria
            {
                Id = 1,
                UsuarioId = 1,
                Nome = "Conta Principal",
                Instituicao = "Banco Padrão",
                Tipo = TipoConta.ContaCorrente,
                SaldoInicial = 0m,
                CorHex = "#2563eb"
            }
        );

        modelBuilder.Entity<Categoria>().HasData(
            new Categoria { Id = 1, UsuarioId = 1, Nome = "Salário & Rendas", CorHex = "#198754", Icone = "bi-wallet2" },
            new Categoria { Id = 2, UsuarioId = 1, Nome = "Alimentação", CorHex = "#fd7e14", Icone = "bi-cart4" },
            new Categoria { Id = 3, UsuarioId = 1, Nome = "Moradia & Contas", CorHex = "#0d6efd", Icone = "bi-house-door" },
            new Categoria { Id = 4, UsuarioId = 1, Nome = "Transporte", CorHex = "#6f42c1", Icone = "bi-fuel-pump" },
            new Categoria { Id = 5, UsuarioId = 1, Nome = "Lazer & Viagens", CorHex = "#e83e8c", Icone = "bi-controller" },
            new Categoria { Id = 6, UsuarioId = 1, Nome = "Saúde & Cuidados", CorHex = "#dc3545", Icone = "bi-heart-pulse" },
            new Categoria { Id = 7, UsuarioId = 1, Nome = "Investimentos", CorHex = "#20c997", Icone = "bi-graph-up-arrow" },
            new Categoria { Id = 8, UsuarioId = 1, Nome = "Outros", CorHex = "#6c757d", Icone = "bi-three-dots" }
        );

        modelBuilder.Entity<CartaoCredito>().HasData(
            new CartaoCredito
            {
                Id = 1,
                UsuarioId = 1,
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
                UsuarioId = 1,
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
