using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Application.Ports;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Ports;

namespace ControleFinanceiro.Application.Services;

public class CartaoService : ICartaoService
{
    private readonly ICartaoCreditoRepository _cartaoRepository;
    private readonly ITransacaoRepository _transacaoRepository;
    private readonly ICategoriaRepository _categoriaRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CartaoService(
        ICartaoCreditoRepository cartaoRepository,
        ITransacaoRepository transacaoRepository,
        ICategoriaRepository categoriaRepository,
        IUnitOfWork unitOfWork)
    {
        _cartaoRepository = cartaoRepository;
        _transacaoRepository = transacaoRepository;
        _categoriaRepository = categoriaRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<CartaoResumoDto>> ListarResumosAsync(int mes, int ano, int usuarioId = 1)
    {
        var cartoes = await _cartaoRepository.ListarTodosOrdenadosPorNomeAsync(incluirTransacoes: true, usuarioId: usuarioId);

        return cartoes.Select(c => new CartaoResumoDto
        {
            Id = c.Id,
            Nome = c.Nome,
            CorHex = c.CorHex,
            LimiteTotal = c.LimiteTotal,
            LimiteUtilizado = c.CalcularLimiteUtilizado(),
            FaturaMesAtual = c.Transacoes
                .Where(t => t.FaturaMes == mes && t.FaturaAno == ano)
                .Sum(t => t.Valor),
            DiaFechamento = c.DiaFechamento,
            DiaVencimento = c.DiaVencimento,
            NomeProgramaPontos = c.AcumulaPontos ? (c.NomeProgramaPontos ?? "Pontos") : "Sem Pontos"
        }).ToList();
    }

    public Task<CartaoCredito?> ObterPorIdAsync(int id, bool incluirTransacoes = false, int usuarioId = 1) =>
        _cartaoRepository.ObterPorIdAsync(id, incluirTransacoes, usuarioId: usuarioId);

    public async Task<ResultadoOperacao> CriarAsync(CartaoCredito cartao)
    {
        await _cartaoRepository.AdicionarAsync(cartao);
        await _unitOfWork.SalvarAlteracoesAsync();
        return ResultadoOperacao.Ok("Cartão de crédito cadastrado com sucesso!");
    }

    public async Task<ResultadoOperacao> AtualizarAsync(CartaoCredito cartao)
    {
        _cartaoRepository.Atualizar(cartao);
        await _unitOfWork.SalvarAlteracoesAsync();
        return ResultadoOperacao.Ok("Cartão de crédito atualizado com sucesso!");
    }

    public async Task<ResultadoOperacao> ExcluirAsync(int id, int usuarioId = 1)
    {
        var cartao = await _cartaoRepository.ObterPorIdAsync(id, incluirTransacoes: true, usuarioId: usuarioId);
        if (cartao == null)
            return ResultadoOperacao.Falha("Cartão não encontrado.");

        if (cartao.Transacoes.Any())
            return ResultadoOperacao.Falha("Não é possível excluir um cartão que possui transações vinculadas.");

        _cartaoRepository.Remover(cartao);
        await _unitOfWork.SalvarAlteracoesAsync();
        return ResultadoOperacao.Ok("Cartão de crédito excluído com sucesso!");
    }

    public async Task<FaturaDto?> ObterFaturaAsync(int cartaoId, int mes, int ano, int usuarioId = 1)
    {
        var cartao = await _cartaoRepository.ObterPorIdAsync(cartaoId, usuarioId: usuarioId);
        if (cartao == null)
            return null;

        var transacoes = await _transacaoRepository.ListarFaturaPorMesAsync(mes, ano, cartaoId, usuarioId: usuarioId);

        return new FaturaDto
        {
            Cartao = cartao,
            Mes = mes,
            Ano = ano,
            Transacoes = transacoes
        };
    }

    public async Task<ResultadoOperacao> PagarFaturaAsync(int cartaoId, int mes, int ano, int usuarioId = 1)
    {
        var cartao = await _cartaoRepository.ObterPorIdAsync(cartaoId, usuarioId: usuarioId);
        if (cartao == null)
            return ResultadoOperacao.Falha("Cartão não encontrado.");

        var transacoes = await _transacaoRepository.ListarFaturaPorMesAsync(mes, ano, cartaoId, apenasNaoPagas: true, usuarioId: usuarioId);
        if (!transacoes.Any())
            return ResultadoOperacao.Falha("Esta fatura já está paga ou não possui lançamentos a quitar.");

        decimal totalFatura = transacoes.Sum(t => t.Valor);

        foreach (var t in transacoes)
        {
            t.FaturaPaga = true;
        }

        var categoriaOutros = await _categoriaRepository.ObterCategoriaPagamentoFaturaAsync(usuarioId);
        int categoriaId = categoriaOutros?.Id ?? 8;

        var pagamentoTransacao = new Transacao
        {
            UsuarioId = usuarioId,
            Descricao = $"Pagamento Fatura {cartao.Nome} ({mes:D2}/{ano})",
            Valor = totalFatura,
            Data = DateTime.Today,
            Tipo = TipoTransacao.Despesa,
            FormaPagamento = FormaPagamento.ContaCorrente,
            CategoriaId = categoriaId,
            Observacao = $"Liquidação automática da fatura de {mes:D2}/{ano} do cartão {cartao.Nome}."
        };

        await _transacaoRepository.AdicionarAsync(pagamentoTransacao);
        await _unitOfWork.SalvarAlteracoesAsync();

        return ResultadoOperacao.Ok($"Fatura de {mes:D2}/{ano} no valor de {totalFatura:C} paga com sucesso!");
    }
}
