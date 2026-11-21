using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Domain.Entities;

namespace ControleFinanceiro.Application.Ports;

public interface IPontosService
{
    Task<PontosPainelDto> ObterPainelPontosAsync(int usuarioId = 1);
    Task<List<CartaoCredito>> ListarCartoesComPontosAsync(int usuarioId = 1);
    Task<ResultadoOperacao> ResgatarPontosAsync(ResgatePontos resgate);
}
