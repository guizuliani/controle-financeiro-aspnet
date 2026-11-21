using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Application.Ports;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Ports;

namespace ControleFinanceiro.Application.Services;

public class TransacaoService : ITransacaoService
{
    private readonly ITransacaoRepository _transacaoRepository;
    private readonly ICartaoCreditoRepository _cartaoRepository;
    private readonly ICategoriaRepository _categoriaRepository;
    private readonly IContaBancariaRepository _contaRepository;
    private readonly IUnitOfWork _unitOfWork;

    public TransacaoService(
        ITransacaoRepository transacaoRepository,
        ICartaoCreditoRepository cartaoRepository,
        ICategoriaRepository categoriaRepository,
        IContaBancariaRepository contaRepository,
        IUnitOfWork unitOfWork)
    {
        _transacaoRepository = transacaoRepository;
        _cartaoRepository = cartaoRepository;
        _categoriaRepository = categoriaRepository;
        _contaRepository = contaRepository;
        _unitOfWork = unitOfWork;
    }

    public Task<List<Transacao>> ListarFiltradasAsync(
        int? mes,
        int? ano,
        TipoTransacao? tipo,
        int? categoriaId,
        int? cartaoId,
        FormaPagamento? formaPagamento,
        string? busca,
        int? contaBancariaId = null,
        int usuarioId = 1) =>
        _transacaoRepository.ListarFiltradasAsync(mes, ano, tipo, categoriaId, cartaoId, formaPagamento, busca, contaBancariaId, usuarioId);

    public Task<Transacao?> ObterPorIdAsync(int id, bool incluirRelacionamentos = false, int usuarioId = 1) =>
        _transacaoRepository.ObterPorIdAsync(id, incluirRelacionamentos, usuarioId);

    public Task<List<Categoria>> ListarCategoriasParaSelecaoAsync(int usuarioId = 1) =>
        _categoriaRepository.ListarTodasOrdenadasPorNomeAsync(usuarioId: usuarioId);

    public Task<List<CartaoCredito>> ListarCartoesParaSelecaoAsync(int usuarioId = 1) =>
        _cartaoRepository.ListarTodosOrdenadosPorNomeAsync(usuarioId: usuarioId);

    public Task<List<ContaBancaria>> ListarContasParaSelecaoAsync(int usuarioId = 1) =>
        _contaRepository.ListarPorUsuarioAsync(usuarioId);

    public async Task<ResultadoOperacao> CriarAsync(Transacao transacao)
    {
        if (transacao.FormaPagamento == FormaPagamento.CartaoCredito)
        {
            if (!transacao.CartaoCreditoId.HasValue || transacao.CartaoCreditoId == 0)
            {
                return ResultadoOperacao.Falha(
                    "Selecione o cartão de crédito para esta forma de pagamento.",
                    nameof(Transacao.CartaoCreditoId));
            }

            var cartao = await _cartaoRepository.ObterPorIdAsync(transacao.CartaoCreditoId.Value, usuarioId: transacao.UsuarioId);
            if (cartao != null)
            {
                var parcelas = Transacao.GerarParcelasCartao(transacao, cartao);
                await _transacaoRepository.AdicionarVariasAsync(parcelas);
                await _unitOfWork.SalvarAlteracoesAsync();

                string msg = parcelas.Count > 1
                    ? $"Compra parcelada em {parcelas.Count}x cadastrada com sucesso!"
                    : "Transação de cartão cadastrada com sucesso!";

                return ResultadoOperacao.Ok(msg);
            }
        }

        transacao.PrepararTransacaoContaCorrente();
        await _transacaoRepository.AdicionarAsync(transacao);
        await _unitOfWork.SalvarAlteracoesAsync();
        return ResultadoOperacao.Ok("Transação cadastrada com sucesso!");
    }

    public async Task<ResultadoOperacao> AtualizarAsync(Transacao transacao)
    {
        if (transacao.FormaPagamento == FormaPagamento.CartaoCredito && transacao.CartaoCreditoId.HasValue)
        {
            var cartao = await _cartaoRepository.ObterPorIdAsync(transacao.CartaoCreditoId.Value, usuarioId: transacao.UsuarioId);
            if (cartao != null)
            {
                transacao.PontosGerados = cartao.CalcularPontos(transacao.Valor);
                transacao.ContaBancariaId = null;
            }
        }
        else
        {
            transacao.CartaoCreditoId = null;
            transacao.PontosGerados = 0;
        }

        _transacaoRepository.Atualizar(transacao);
        await _unitOfWork.SalvarAlteracoesAsync();
        return ResultadoOperacao.Ok("Transação atualizada com sucesso!");
    }

    public async Task<ResultadoOperacao> ExcluirAsync(int id, int usuarioId = 1)
    {
        var transacao = await _transacaoRepository.ObterPorIdAsync(id, usuarioId: usuarioId);
        if (transacao == null)
            return ResultadoOperacao.Falha("Transação não encontrada.");

        _transacaoRepository.Remover(transacao);
        await _unitOfWork.SalvarAlteracoesAsync();
        return ResultadoOperacao.Ok("Transação excluída com sucesso!");
    }
}
