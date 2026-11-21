using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Application.Ports;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Ports;

namespace ControleFinanceiro.Application.Services;

public class ExtratoPortabilidadeService : IExtratoPortabilidadeService
{
    private readonly ITransacaoRepository _transacaoRepository;
    private readonly ICartaoCreditoRepository _cartaoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ExtratoPortabilidadeService(
        ITransacaoRepository transacaoRepository,
        ICartaoCreditoRepository cartaoRepository,
        IUnitOfWork unitOfWork)
    {
        _transacaoRepository = transacaoRepository;
        _cartaoRepository = cartaoRepository;
        _unitOfWork = unitOfWork;
    }

    public byte[] ExportarCsv(IEnumerable<Transacao> transacoes)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Data;Descricao;Tipo;Categoria;FormaPagamento;ContaOuCartao;Valor;Parcela;Pontos");

        foreach (var t in transacoes)
        {
            string origem = t.FormaPagamento == FormaPagamento.CartaoCredito
                ? t.CartaoCredito?.Nome ?? "Cartão"
                : t.ContaBancaria?.Nome ?? "Conta Corrente";

            string descricaoLimpa = t.Descricao.Replace(";", ",");
            string categoria = (t.Categoria?.Nome ?? "Sem Categoria").Replace(";", ",");

            sb.AppendLine(string.Join(";",
                t.Data.ToString("yyyy-MM-dd"),
                descricaoLimpa,
                t.Tipo.ToString(),
                categoria,
                t.FormaPagamento.ToString(),
                origem.Replace(";", ","),
                t.Valor.ToString("F2", CultureInfo.InvariantCulture),
                $"{t.NumeroParcela}/{t.TotalParcelas}",
                t.PontosGerados.ToString("F2", CultureInfo.InvariantCulture)));
        }

        // UTF-8 com BOM para abertura direta no Excel sem quebrar acentos
        var bom = Encoding.UTF8.GetPreamble();
        var body = Encoding.UTF8.GetBytes(sb.ToString());
        return bom.Concat(body).ToArray();
    }

    public IReadOnlyList<ItemImportacaoDto> LerArquivoCsv(string conteudoCsv)
    {
        var itens = new List<ItemImportacaoDto>();
        if (string.IsNullOrWhiteSpace(conteudoCsv))
            return itens;

        var linhas = conteudoCsv.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        foreach (var linha in linhas.Skip(1)) // Pula cabeçalho
        {
            char separador = linha.Contains(';') ? ';' : ',';
            var colunas = linha.Split(separador);
            if (colunas.Length < 3) continue;

            if (!DateTime.TryParse(colunas[0].Trim(), new CultureInfo("pt-BR"), DateTimeStyles.None, out var data) &&
                !DateTime.TryParse(colunas[0].Trim(), CultureInfo.InvariantCulture, DateTimeStyles.None, out data))
            {
                continue;
            }

            string descricao = colunas[1].Trim().Trim('"');
            if (string.IsNullOrWhiteSpace(descricao))
                descricao = "Lançamento Importado";

            string valorStr = colunas[2].Trim().Trim('"').Replace("R$", "").Trim();
            if (!decimal.TryParse(valorStr, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal valor) &&
                !decimal.TryParse(valorStr, NumberStyles.Any, new CultureInfo("pt-BR"), out valor))
            {
                continue;
            }

            var tipo = valor >= 0 ? TipoTransacao.Receita : TipoTransacao.Despesa;
            if (colunas.Length >= 4 && colunas[3].Trim().Contains("Despesa", StringComparison.OrdinalIgnoreCase))
                tipo = TipoTransacao.Despesa;

            itens.Add(new ItemImportacaoDto
            {
                Data = data,
                Descricao = descricao.Length > 100 ? descricao[..100] : descricao,
                Valor = Math.Abs(valor),
                Tipo = tipo
            });
        }

        return itens;
    }

    public IReadOnlyList<ItemImportacaoDto> LerArquivoOfx(string conteudoOfx)
    {
        var itens = new List<ItemImportacaoDto>();
        if (string.IsNullOrWhiteSpace(conteudoOfx))
            return itens;

        var blocos = Regex.Matches(conteudoOfx, @"<STMTTRN>(.*?)</STMTTRN>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
        foreach (Match bloco in blocos)
        {
            string corpo = bloco.Groups[1].Value;
            string dtStr = ExtrairTagOfx(corpo, "DTPOSTED");
            string amtStr = ExtrairTagOfx(corpo, "TRNAMT");
            string memo = ExtrairTagOfx(corpo, "MEMO");
            if (string.IsNullOrWhiteSpace(memo))
                memo = ExtrairTagOfx(corpo, "NAME");

            if (dtStr.Length < 8 || !DateTime.TryParseExact(dtStr[..8], "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var data))
                data = DateTime.Today;

            if (!decimal.TryParse(amtStr.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var valor))
                continue;

            itens.Add(new ItemImportacaoDto
            {
                Data = data,
                Descricao = string.IsNullOrWhiteSpace(memo) ? "Transação OFX" : (memo.Length > 100 ? memo[..100] : memo),
                Valor = Math.Abs(valor),
                Tipo = valor < 0 ? TipoTransacao.Despesa : TipoTransacao.Receita
            });
        }

        return itens;
    }

    public async Task<ResultadoOperacao> ImportarItensAsync(
        IEnumerable<ItemImportacaoDto> itens,
        int categoriaId,
        FormaPagamento formaPagamento,
        int? contaBancariaId,
        int? cartaoCreditoId,
        int usuarioId = 1)
    {
        var lista = itens.ToList();
        if (!lista.Any())
            return ResultadoOperacao.Falha("Nenhum lançamento válido encontrado no arquivo enviado.");

        CartaoCredito? cartao = null;
        if (formaPagamento == FormaPagamento.CartaoCredito)
        {
            if (!cartaoCreditoId.HasValue || cartaoCreditoId == 0)
                return ResultadoOperacao.Falha("Selecione um cartão de crédito para importar a fatura.");

            cartao = await _cartaoRepository.ObterPorIdAsync(cartaoCreditoId.Value, usuarioId: usuarioId);
            if (cartao == null)
                return ResultadoOperacao.Falha("Cartão de crédito não encontrado.");
        }

        var transacoes = new List<Transacao>();
        foreach (var item in lista)
        {
            if (item.Valor <= 0) continue;

            if (formaPagamento == FormaPagamento.CartaoCredito && cartao != null)
            {
                var baseCompra = new Transacao
                {
                    UsuarioId = usuarioId,
                    Descricao = item.Descricao,
                    Valor = item.Valor,
                    Data = item.Data,
                    Tipo = TipoTransacao.Despesa,
                    CategoriaId = categoriaId,
                    FormaPagamento = FormaPagamento.CartaoCredito,
                    CartaoCreditoId = cartao.Id,
                    TotalParcelas = 1,
                    Observacao = "Importado via arquivo"
                };
                transacoes.AddRange(Transacao.GerarParcelasCartao(baseCompra, cartao));
            }
            else
            {
                var transacao = new Transacao
                {
                    UsuarioId = usuarioId,
                    Descricao = item.Descricao,
                    Valor = item.Valor,
                    Data = item.Data,
                    Tipo = item.Tipo,
                    CategoriaId = categoriaId,
                    FormaPagamento = FormaPagamento.ContaCorrente,
                    ContaBancariaId = contaBancariaId,
                    Observacao = "Importado via arquivo"
                };
                transacao.PrepararTransacaoContaCorrente();
                transacoes.Add(transacao);
            }
        }

        if (!transacoes.Any())
            return ResultadoOperacao.Falha("Nenhuma transação válida pôde ser importada.");

        await _transacaoRepository.AdicionarVariasAsync(transacoes);
        await _unitOfWork.SalvarAlteracoesAsync();

        return ResultadoOperacao.Ok($"{transacoes.Count} lançamento(s) importado(s) com sucesso!");
    }

    private static string ExtrairTagOfx(string texto, string tag)
    {
        var match = Regex.Match(texto, $@"<{tag}>([^<\r\n]+)", RegexOptions.IgnoreCase);
        return match.Success ? match.Groups[1].Value.Trim() : string.Empty;
    }
}
