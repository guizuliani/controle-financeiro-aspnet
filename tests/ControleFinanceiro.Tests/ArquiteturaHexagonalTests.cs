using ControleFinanceiro.Application.Ports;
using ControleFinanceiro.Controllers;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Infrastructure.Persistence;
using NetArchTest.Rules;
using Xunit;

namespace ControleFinanceiro.Tests;

public class ArquiteturaHexagonalTests
{
    [Fact]
    public void Domain_NaoDeveDependerDeCamadasExternas()
    {
        var result = Types.InAssembly(typeof(CartaoCredito).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "ControleFinanceiro.Application",
                "ControleFinanceiro.Infrastructure",
                "ControleFinanceiro.Controllers",
                "Microsoft.EntityFrameworkCore")
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void Application_NaoDeveDependerDeInfrastructureOuWeb()
    {
        var result = Types.InAssembly(typeof(IDashboardService).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "ControleFinanceiro.Infrastructure",
                "ControleFinanceiro.Controllers",
                "Microsoft.EntityFrameworkCore")
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void Controllers_NaoDevemDependerDeInfrastructureOuDbContext()
    {
        var result = Types.InAssembly(typeof(HomeController).Assembly)
            .That()
            .ResideInNamespace("ControleFinanceiro.Controllers")
            .ShouldNot()
            .HaveDependencyOnAny(
                "ControleFinanceiro.Infrastructure.Persistence",
                "ControleFinanceiro.Infrastructure.Repositories")
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join(", ", result.FailingTypeNames ?? []));
    }
}
