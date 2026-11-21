using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Application.Ports;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Ports;

namespace ControleFinanceiro.Application.Services;

public class OrcamentoService : IOrcamentoService
{
    private readonly IOrcamentoRepository _orcamentoRepository;
    private readonly ITransacaoRepository _transacaoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public OrcamentoService(
        IOrcamentoRepository orcamentoRepository,
        ITransacaoRepository transacaoRepository,
        IUnitOfWork unitOfWork)
    {
        _orcamentoRepository = orcamentoRepository;
        _transacaoRepository = transacaoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<OrcamentoResumoDto>> ListarResumosMesAsync(int mes, int ano, int usuarioId = 1)
    {
        var orcamentos = await _orcamentoRepository.ListarPorMesAsync(usuarioId, mes, ano);
        if (!orcamentos.Any())
            return new List<OrcamentoResumoDto>();

        var despesasMes = await _transacaoRepository.ListarDespesasCompetenciaMesAsync(mes, ano, usuarioId);
        var gastosPorCategoria = despesasMes
            .GroupBy(t => t.CategoriaId)
            .ToDictionary(g => g.Key, g => g.Sum(t => t.Valor));

        return orcamentos.Select(o =>
        {
            var gasto = gastosPorCategoria.GetValueOrDefault(o.CategoriaId, 0m);
            return new OrcamentoResumoDto
            {
                Id = o.Id,
                CategoriaId = o.CategoriaId,
                CategoriaNome = o.Categoria?.Nome ?? "Categoria",
                CategoriaCorHex = o.Categoria?.CorHex ?? "#6c757d",
                ValorLimite = o.ValorLimite,
                ValorGasto = gasto,
                PercentualUso = o.CalcularPercentualUso(gasto),
                NivelAlerta = o.ObterNivelAlerta(gasto),
                Mes = o.Mes,
                Ano = o.Ano
            };
        }).OrderByDescending(x => x.PercentualUso).ToList();
    }

    public Task<OrcamentoCategoria?> ObterPorIdAsync(int id, int usuarioId = 1) =>
        _orcamentoRepository.ObterPorIdAsync(id, usuarioId);

    public async Task<ResultadoOperacao> SalvarAsync(OrcamentoCategoria orcamento)
    {
        var existente = await _orcamentoRepository.ObterPorCategoriaEMesAsync(
            orcamento.UsuarioId,
            orcamento.CategoriaId,
            orcamento.Mes,
            orcamento.Ano);

        if (existente != null && existente.Id != orcamento.Id)
        {
            existente.ValorLimite = orcamento.ValorLimite;
            _orcamentoRepository.Atualizar(existente);
            await _unitOfWork.SalvarAlteracoesAsync();
            return ResultadoOperacao.Ok("Orçamento atualizado com sucesso!");
        }

        if (orcamento.Id == 0)
            await _orcamentoRepository.AdicionarAsync(orcamento);
        else
            _orcamentoRepository.Atualizar(orcamento);

        await _unitOfWork.SalvarAlteracoesAsync();
        return ResultadoOperacao.Ok("Orçamento salvo com sucesso!");
    }

    public async Task<ResultadoOperacao> ExcluirAsync(int id, int usuarioId = 1)
    {
        var orcamento = await _orcamentoRepository.ObterPorIdAsync(id, usuarioId);
        if (orcamento == null)
            return ResultadoOperacao.Falha("Orçamento não encontrado.");

        _orcamentoRepository.Remover(orcamento);
        await _unitOfWork.SalvarAlteracoesAsync();
        return ResultadoOperacao.Ok("Orçamento removido com sucesso!");
    }
}
