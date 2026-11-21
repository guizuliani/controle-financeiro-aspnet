namespace ControleFinanceiro.Domain.Ports;

public interface IUnitOfWork
{
    Task<int> SalvarAlteracoesAsync();
}
