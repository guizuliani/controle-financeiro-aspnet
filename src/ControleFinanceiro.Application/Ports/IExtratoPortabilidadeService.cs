using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Enums;

namespace ControleFinanceiro.Application.Ports;

public interface IExtratoPortabilidadeService
{
    byte[] ExportarCsv(IEnumerable<Transacao> transacoes);
    IReadOnlyList<ItemImportacaoDto> LerArquivoCsv(string conteudoCsv);
    IReadOnlyList<ItemImportacaoDto> LerArquivoOfx(string conteudoOfx);
    Task<ResultadoOperacao> ImportarItensAsync(
        IEnumerable<ItemImportacaoDto> itens,
        int categoriaId,
        FormaPagamento formaPagamento,
        int? contaBancariaId,
        int? cartaoCreditoId,
        int usuarioId = 1);
}
