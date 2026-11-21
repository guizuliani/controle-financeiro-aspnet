using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Application.Ports;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Ports;

namespace ControleFinanceiro.Application.Services;

public class PontosService : IPontosService
{
    private readonly ICartaoCreditoRepository _cartaoRepository;
    private readonly ITransacaoRepository _transacaoRepository;
    private readonly IResgatePontosRepository _resgateRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PontosService(
        ICartaoCreditoRepository cartaoRepository,
        ITransacaoRepository transacaoRepository,
        IResgatePontosRepository resgateRepository,
        IUnitOfWork unitOfWork)
    {
        _cartaoRepository = cartaoRepository;
        _transacaoRepository = transacaoRepository;
        _resgateRepository = resgateRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<PontosPainelDto> ObterPainelPontosAsync()
    {
        var cartoes = await _cartaoRepository.ListarComPontosAsync(incluirTransacoesEResgates: true);

        var programas = cartoes.Select(c => new ProgramaPontosDto
        {
            CartaoId = c.Id,
            CartaoNome = c.Nome,
            NomePrograma = c.NomeProgramaPontos ?? "Programa de Pontos",
            CorHex = c.CorHex,
            TotalAcumulado = c.Transacoes.Sum(t => t.PontosGerados),
            TotalResgatado = c.Resgates.Sum(r => r.Pontos),
            RegraDescricao = c.ObterDescricaoRegraPontos()
        }).ToList();

        var extratoTransacoes = await _transacaoRepository.ListarComPontosAsync(25);
        var historicoResgates = await _resgateRepository.ListarUltimosAsync(25);

        return new PontosPainelDto
        {
            Programas = programas,
            ExtratoTransacoes = extratoTransacoes,
            HistoricoResgates = historicoResgates
        };
    }

    public Task<List<CartaoCredito>> ListarCartoesComPontosAsync() =>
        _cartaoRepository.ListarComPontosAsync();

    public async Task<ResultadoOperacao> ResgatarPontosAsync(ResgatePontos resgate)
    {
        var cartao = await _cartaoRepository.ObterPorIdAsync(
            resgate.CartaoCreditoId,
            incluirTransacoes: true,
            incluirResgates: true);

        if (cartao != null)
        {
            var saldoDisponivel = cartao.CalcularSaldoPontos();
            if (resgate.Pontos > saldoDisponivel)
            {
                return ResultadoOperacao.Falha(
                    $"Saldo insuficiente! Saldo disponível neste cartão: {saldoDisponivel:N0} pontos.",
                    nameof(ResgatePontos.Pontos));
            }
        }

        await _resgateRepository.AdicionarAsync(resgate);
        await _unitOfWork.SalvarAlteracoesAsync();

        return ResultadoOperacao.Ok($"Resgate de {resgate.Pontos:N0} pontos registrado com sucesso!");
    }
}
