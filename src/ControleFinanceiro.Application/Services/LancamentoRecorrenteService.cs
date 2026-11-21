using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Application.Ports;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Ports;

namespace ControleFinanceiro.Application.Services;

public class LancamentoRecorrenteService : ILancamentoRecorrenteService
{
    private readonly ILancamentoRecorrenteRepository _recorrenteRepository;
    private readonly ITransacaoRepository _transacaoRepository;
    private readonly ICartaoCreditoRepository _cartaoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public LancamentoRecorrenteService(
        ILancamentoRecorrenteRepository recorrenteRepository,
        ITransacaoRepository transacaoRepository,
        ICartaoCreditoRepository cartaoRepository,
        IUnitOfWork unitOfWork)
    {
        _recorrenteRepository = recorrenteRepository;
        _transacaoRepository = transacaoRepository;
        _cartaoRepository = cartaoRepository;
        _unitOfWork = unitOfWork;
    }

    public Task<List<LancamentoRecorrente>> ListarPorUsuarioAsync(int usuarioId = 1) =>
        _recorrenteRepository.ListarPorUsuarioAsync(usuarioId);

    public Task<LancamentoRecorrente?> ObterPorIdAsync(int id, int usuarioId = 1) =>
        _recorrenteRepository.ObterPorIdAsync(id, usuarioId);

    public async Task<ResultadoOperacao> CriarAsync(LancamentoRecorrente recorrente)
    {
        if (recorrente.FormaPagamento == FormaPagamento.CartaoCredito && (!recorrente.CartaoCreditoId.HasValue || recorrente.CartaoCreditoId == 0))
            return ResultadoOperacao.Falha("Selecione o cartão de crédito para esta recorrência.", nameof(LancamentoRecorrente.CartaoCreditoId));

        if (recorrente.FormaPagamento == FormaPagamento.ContaCorrente)
            recorrente.CartaoCreditoId = null;

        await _recorrenteRepository.AdicionarAsync(recorrente);
        await _unitOfWork.SalvarAlteracoesAsync();
        return ResultadoOperacao.Ok("Lançamento recorrente cadastrado com sucesso!");
    }

    public async Task<ResultadoOperacao> AlternarStatusAsync(int id, int usuarioId = 1)
    {
        var item = await _recorrenteRepository.ObterPorIdAsync(id, usuarioId);
        if (item == null)
            return ResultadoOperacao.Falha("Lançamento recorrente não encontrado.");

        item.Ativo = !item.Ativo;
        _recorrenteRepository.Atualizar(item);
        await _unitOfWork.SalvarAlteracoesAsync();
        return ResultadoOperacao.Ok(item.Ativo ? "Recorrência ativada." : "Recorrência pausada.");
    }

    public async Task<ResultadoOperacao> ExcluirAsync(int id, int usuarioId = 1)
    {
        var item = await _recorrenteRepository.ObterPorIdAsync(id, usuarioId);
        if (item == null)
            return ResultadoOperacao.Falha("Lançamento recorrente não encontrado.");

        _recorrenteRepository.Remover(item);
        await _unitOfWork.SalvarAlteracoesAsync();
        return ResultadoOperacao.Ok("Lançamento recorrente excluído com sucesso!");
    }

    public async Task<ResultadoOperacao> GerarLancamentosDoMesAsync(int mes, int ano, int usuarioId = 1)
    {
        var ativos = await _recorrenteRepository.ListarAtivosPorUsuarioAsync(usuarioId);
        int gerados = 0;

        foreach (var rec in ativos)
        {
            if (rec.JaGeradoParaMes(mes, ano))
                continue;

            var dataCompetencia = rec.CalcularDataCompetencia(mes, ano);

            if (rec.FormaPagamento == FormaPagamento.CartaoCredito && rec.CartaoCreditoId.HasValue)
            {
                var cartao = await _cartaoRepository.ObterPorIdAsync(rec.CartaoCreditoId.Value, usuarioId: usuarioId);
                if (cartao != null)
                {
                    var transacaoBase = new Transacao
                    {
                        UsuarioId = usuarioId,
                        Descricao = rec.Descricao,
                        Valor = rec.Valor,
                        Data = dataCompetencia,
                        Tipo = TipoTransacao.Despesa,
                        CategoriaId = rec.CategoriaId,
                        FormaPagamento = FormaPagamento.CartaoCredito,
                        CartaoCreditoId = cartao.Id,
                        TotalParcelas = 1,
                        Observacao = "Gerado automaticamente via Lançamento Recorrente"
                    };
                    var parcelas = Transacao.GerarParcelasCartao(transacaoBase, cartao);
                    await _transacaoRepository.AdicionarVariasAsync(parcelas);
                    gerados++;
                }
            }
            else
            {
                var transacao = new Transacao
                {
                    UsuarioId = usuarioId,
                    Descricao = rec.Descricao,
                    Valor = rec.Valor,
                    Data = dataCompetencia,
                    Tipo = rec.Tipo,
                    CategoriaId = rec.CategoriaId,
                    FormaPagamento = FormaPagamento.ContaCorrente,
                    ContaBancariaId = rec.ContaBancariaId,
                    Observacao = "Gerado automaticamente via Lançamento Recorrente"
                };
                transacao.PrepararTransacaoContaCorrente();
                await _transacaoRepository.AdicionarAsync(transacao);
                gerados++;
            }

            rec.UltimoMesGerado = mes;
            rec.UltimoAnoGerado = ano;
            _recorrenteRepository.Atualizar(rec);
        }

        if (gerados > 0)
            await _unitOfWork.SalvarAlteracoesAsync();

        return ResultadoOperacao.Ok(gerados > 0
            ? $"{gerados} lançamento(s) recorrente(s) gerado(s) para {mes:D2}/{ano}!"
            : $"Todos os lançamentos ativos de {mes:D2}/{ano} já foram gerados anteriormente.");
    }
}
