using System.Text;
using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Application.Services;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Infrastructure.Persistence;
using ControleFinanceiro.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ControleFinanceiro.Tests;

public class NovosRecursosApplicationTests
{
    private static AppDbContext CriarContexto(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        var context = new AppDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    [Fact]
    public async Task AuthService_RegistrarEAutenticar_DeveIsolarNovoUsuario()
    {
        using var context = CriarContexto("Db_Auth");
        var authService = new AuthService(
            new UsuarioRepository(context),
            new CategoriaRepository(context),
            new ContaBancariaRepository(context),
            new UnitOfWork(context));

        var registro = await authService.RegistrarAsync("Guilherme", "gui@teste.com", "Senha@123");
        Assert.True(registro.Sucesso);
        Assert.NotNull(registro.Usuario);

        var loginOk = await authService.AutenticarAsync("gui@teste.com", "Senha@123");
        Assert.True(loginOk.Sucesso);

        var loginErrado = await authService.AutenticarAsync("gui@teste.com", "Errada");
        Assert.False(loginErrado.Sucesso);

        // Deve ter criado categorias iniciais e conta principal para o novo usuário
        var contas = await context.ContasBancarias.Where(c => c.UsuarioId == registro.Usuario!.Id).ToListAsync();
        Assert.Single(contas);
    }

    [Fact]
    public async Task ContaBancaria_CalcularSaldoAtual_DeveSomarReceitasESubtrairDespesas()
    {
        using var context = CriarContexto("Db_Contas");
        var repo = new ContaBancariaRepository(context);
        var uow = new UnitOfWork(context);
        var service = new ContaBancariaService(repo, uow);

        var conta = new ContaBancaria
        {
            UsuarioId = 1,
            Nome = "Reserva",
            SaldoInicial = 1000m,
            Tipo = TipoConta.Poupanca
        };
        await service.CriarAsync(conta);

        context.Transacoes.AddRange(
            new Transacao { UsuarioId = 1, Descricao = "Depósito", Valor = 500m, Tipo = TipoTransacao.Receita, FormaPagamento = FormaPagamento.ContaCorrente, ContaBancariaId = conta.Id, CategoriaId = 1 },
            new Transacao { UsuarioId = 1, Descricao = "Saque", Valor = 200m, Tipo = TipoTransacao.Despesa, FormaPagamento = FormaPagamento.ContaCorrente, ContaBancariaId = conta.Id, CategoriaId = 2 }
        );
        await context.SaveChangesAsync();

        var resumos = await service.ListarResumosAsync(1);
        var reserva = resumos.First(c => c.Nome == "Reserva");
        Assert.Equal(1300m, reserva.SaldoAtual);
    }

    [Fact]
    public async Task OrcamentoService_DeveCalcularPercentualENivelAlerta()
    {
        using var context = CriarContexto("Db_Orcamentos");
        var service = new OrcamentoService(
            new OrcamentoRepository(context),
            new TransacaoRepository(context),
            new UnitOfWork(context));

        await service.SalvarAsync(new OrcamentoCategoria
        {
            UsuarioId = 1,
            CategoriaId = 2,
            ValorLimite = 1000m,
            Mes = 11,
            Ano = 2026
        });

        context.Transacoes.Add(new Transacao
        {
            UsuarioId = 1,
            Descricao = "Supermercado",
            Valor = 850m,
            Data = new DateTime(2026, 11, 10),
            Tipo = TipoTransacao.Despesa,
            FormaPagamento = FormaPagamento.ContaCorrente,
            CategoriaId = 2
        });
        await context.SaveChangesAsync();

        var resumos = await service.ListarResumosMesAsync(11, 2026, 1);
        Assert.Single(resumos);
        Assert.Equal(85.0m, resumos[0].PercentualUso);
        Assert.Equal("Atencao", resumos[0].NivelAlerta);
    }

    [Fact]
    public async Task LancamentoRecorrenteService_DeveGerarTransacoesApenasUmaVezPorMes()
    {
        using var context = CriarContexto("Db_Recorrencias");
        var service = new LancamentoRecorrenteService(
            new LancamentoRecorrenteRepository(context),
            new TransacaoRepository(context),
            new CartaoCreditoRepository(context),
            new UnitOfWork(context));

        await service.CriarAsync(new LancamentoRecorrente
        {
            UsuarioId = 1,
            Descricao = "Internet Fibra",
            Valor = 120m,
            Tipo = TipoTransacao.Despesa,
            CategoriaId = 3,
            FormaPagamento = FormaPagamento.ContaCorrente,
            DiaDoMes = 10
        });

        var primeiraExec = await service.GerarLancamentosDoMesAsync(11, 2026, 1);
        Assert.Contains("1 lançamento(s)", primeiraExec.Mensagem);

        var segundaExec = await service.GerarLancamentosDoMesAsync(11, 2026, 1);
        Assert.Contains("já foram gerados", segundaExec.Mensagem);

        Assert.Equal(1, await context.Transacoes.CountAsync(t => t.Descricao == "Internet Fibra"));
    }

    [Fact]
    public async Task ExtratoPortabilidadeService_DeveLerOfxECsvEExportarCsv()
    {
        using var context = CriarContexto("Db_Importacao");
        var service = new ExtratoPortabilidadeService(
            new TransacaoRepository(context),
            new CartaoCreditoRepository(context),
            new UnitOfWork(context));

        var csv = "Data;Descricao;Valor\n2026-11-10;Padaria;-25.50\n2026-11-11;Pix Recebido;100.00";
        var itensCsv = service.LerArquivoCsv(csv);
        Assert.Equal(2, itensCsv.Count);
        Assert.Equal(TipoTransacao.Despesa, itensCsv[0].Tipo);
        Assert.Equal(25.50m, itensCsv[0].Valor);

        var ofx = "<OFX><STMTTRN><TRNTYPE>DEBIT<DTPOSTED>20261112120000<TRNAMT>-89.90<MEMO>Farmacia</STMTTRN></OFX>";
        var itensOfx = service.LerArquivoOfx(ofx);
        Assert.Single(itensOfx);
        Assert.Equal("Farmacia", itensOfx[0].Descricao);
        Assert.Equal(89.90m, itensOfx[0].Valor);

        var res = await service.ImportarItensAsync(itensOfx, 6, FormaPagamento.ContaCorrente, 1, null, 1);
        Assert.True(res.Sucesso);

        var exportBytes = service.ExportarCsv(await context.Transacoes.Include(t => t.Categoria).ToListAsync());
        var textoExport = Encoding.UTF8.GetString(exportBytes);
        Assert.Contains("Farmacia", textoExport);
    }
}
