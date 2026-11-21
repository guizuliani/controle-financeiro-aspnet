using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Domain.Entities;

namespace ControleFinanceiro.Application.Ports;

public interface IPontosService
{
    Task<PontosPainelDto> ObterPainelPontosAsync();
    Task<List<CartaoCredito>> ListarCartoesComPontosAsync();
    Task<ResultadoOperacao> ResgatarPontosAsync(ResgatePontos resgate);
}
