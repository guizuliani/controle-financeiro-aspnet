using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Application.Ports;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Ports;

namespace ControleFinanceiro.Application.Services;

public class ContaBancariaService : IContaBancariaService
{
    private readonly IContaBancariaRepository _contaRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ContaBancariaService(IContaBancariaRepository contaRepository, IUnitOfWork unitOfWork)
    {
        _contaRepository = contaRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<ContaBancariaResumoDto>> ListarResumosAsync(int usuarioId = 1)
    {
        var contas = await _contaRepository.ListarPorUsuarioAsync(usuarioId, incluirTransacoes: true);
        return contas.Select(c => new ContaBancariaResumoDto
        {
            Id = c.Id,
            Nome = c.Nome,
            Instituicao = c.Instituicao,
            Tipo = c.Tipo,
            SaldoInicial = c.SaldoInicial,
            SaldoAtual = c.CalcularSaldoAtual(),
            CorHex = c.CorHex
        }).ToList();
    }

    public Task<ContaBancaria?> ObterPorIdAsync(int id, int usuarioId = 1) =>
        _contaRepository.ObterPorIdAsync(id, usuarioId, incluirTransacoes: true);

    public async Task<ResultadoOperacao> CriarAsync(ContaBancaria conta)
    {
        await _contaRepository.AdicionarAsync(conta);
        await _unitOfWork.SalvarAlteracoesAsync();
        return ResultadoOperacao.Ok("Conta bancária cadastrada com sucesso!");
    }

    public async Task<ResultadoOperacao> AtualizarAsync(ContaBancaria conta)
    {
        _contaRepository.Atualizar(conta);
        await _unitOfWork.SalvarAlteracoesAsync();
        return ResultadoOperacao.Ok("Conta bancária atualizada com sucesso!");
    }

    public async Task<ResultadoOperacao> ExcluirAsync(int id, int usuarioId = 1)
    {
        var conta = await _contaRepository.ObterPorIdAsync(id, usuarioId, incluirTransacoes: true);
        if (conta == null)
            return ResultadoOperacao.Falha("Conta bancária não encontrada.");

        if (conta.Transacoes.Any())
            return ResultadoOperacao.Falha("Não é possível excluir uma conta que possui transações vinculadas.");

        _contaRepository.Remover(conta);
        await _unitOfWork.SalvarAlteracoesAsync();
        return ResultadoOperacao.Ok("Conta bancária excluída com sucesso!");
    }
}
