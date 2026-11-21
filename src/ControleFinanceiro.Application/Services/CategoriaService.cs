using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Application.Ports;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Ports;

namespace ControleFinanceiro.Application.Services;

public class CategoriaService : ICategoriaService
{
    private readonly ICategoriaRepository _categoriaRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CategoriaService(ICategoriaRepository categoriaRepository, IUnitOfWork unitOfWork)
    {
        _categoriaRepository = categoriaRepository;
        _unitOfWork = unitOfWork;
    }

    public Task<List<Categoria>> ListarTodasAsync(bool incluirTransacoes = false) =>
        _categoriaRepository.ListarTodasOrdenadasPorNomeAsync(incluirTransacoes);

    public Task<Categoria?> ObterPorIdAsync(int id, bool incluirTransacoes = false) =>
        _categoriaRepository.ObterPorIdAsync(id, incluirTransacoes);

    public async Task<ResultadoOperacao> CriarAsync(Categoria categoria)
    {
        await _categoriaRepository.AdicionarAsync(categoria);
        await _unitOfWork.SalvarAlteracoesAsync();
        return ResultadoOperacao.Ok("Categoria criada com sucesso!");
    }

    public async Task<ResultadoOperacao> AtualizarAsync(Categoria categoria)
    {
        _categoriaRepository.Atualizar(categoria);
        await _unitOfWork.SalvarAlteracoesAsync();
        return ResultadoOperacao.Ok("Categoria atualizada com sucesso!");
    }

    public async Task<ResultadoOperacao> ExcluirAsync(int id)
    {
        var categoria = await _categoriaRepository.ObterPorIdAsync(id, incluirTransacoes: true);
        if (categoria == null)
            return ResultadoOperacao.Falha("Categoria não encontrada.");

        if (categoria.PossuiTransacoesVinculadas())
            return ResultadoOperacao.Falha("Não é possível excluir uma categoria que possui transações vinculadas.");

        _categoriaRepository.Remover(categoria);
        await _unitOfWork.SalvarAlteracoesAsync();
        return ResultadoOperacao.Ok("Categoria excluída com sucesso!");
    }
}
