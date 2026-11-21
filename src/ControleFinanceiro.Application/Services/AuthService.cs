using System.Security.Cryptography;
using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Application.Ports;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Ports;

namespace ControleFinanceiro.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly ICategoriaRepository _categoriaRepository;
    private readonly IContaBancariaRepository _contaRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AuthService(
        IUsuarioRepository usuarioRepository,
        ICategoriaRepository categoriaRepository,
        IContaBancariaRepository contaRepository,
        IUnitOfWork unitOfWork)
    {
        _usuarioRepository = usuarioRepository;
        _categoriaRepository = categoriaRepository;
        _contaRepository = contaRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ResultadoAuth> RegistrarAsync(string nome, string email, string senha)
    {
        var emailNormalizado = email.Trim().ToLowerInvariant();
        if (await _usuarioRepository.EmailExisteAsync(emailNormalizado))
            return ResultadoAuth.Falha("Já existe uma conta cadastrada com este e-mail.");

        var usuario = new Usuario
        {
            Nome = nome.Trim(),
            Email = emailNormalizado,
            SenhaHash = GerarHashSenha(senha),
            CriadoEm = DateTime.UtcNow
        };

        await _usuarioRepository.AdicionarAsync(usuario);
        await _unitOfWork.SalvarAlteracoesAsync();

        // Criar categorias iniciais e conta principal para o novo usuário
        var categoriasIniciais = new List<Categoria>
        {
            new() { UsuarioId = usuario.Id, Nome = "Salário & Rendas", CorHex = "#198754", Icone = "bi-wallet2" },
            new() { UsuarioId = usuario.Id, Nome = "Alimentação", CorHex = "#fd7e14", Icone = "bi-cart4" },
            new() { UsuarioId = usuario.Id, Nome = "Moradia & Contas", CorHex = "#0d6efd", Icone = "bi-house-door" },
            new() { UsuarioId = usuario.Id, Nome = "Transporte", CorHex = "#6f42c1", Icone = "bi-fuel-pump" },
            new() { UsuarioId = usuario.Id, Nome = "Lazer & Viagens", CorHex = "#e83e8c", Icone = "bi-controller" },
            new() { UsuarioId = usuario.Id, Nome = "Saúde & Cuidados", CorHex = "#dc3545", Icone = "bi-heart-pulse" },
            new() { UsuarioId = usuario.Id, Nome = "Investimentos", CorHex = "#20c997", Icone = "bi-graph-up-arrow" },
            new() { UsuarioId = usuario.Id, Nome = "Outros", CorHex = "#6c757d", Icone = "bi-three-dots" }
        };

        await _categoriaRepository.AdicionarVariasAsync(categoriasIniciais);
        await _contaRepository.AdicionarAsync(new ContaBancaria
        {
            UsuarioId = usuario.Id,
            Nome = "Conta Principal",
            Instituicao = "Banco Padrão",
            Tipo = TipoConta.ContaCorrente,
            SaldoInicial = 0m,
            CorHex = "#2563eb"
        });

        await _unitOfWork.SalvarAlteracoesAsync();

        return ResultadoAuth.Ok(new UsuarioAutenticadoDto(usuario.Id, usuario.Nome, usuario.Email), "Conta criada com sucesso!");
    }

    public async Task<ResultadoAuth> AutenticarAsync(string email, string senha)
    {
        var emailNormalizado = email.Trim().ToLowerInvariant();
        var usuario = await _usuarioRepository.ObterPorEmailAsync(emailNormalizado);

        if (usuario == null || !VerificarSenha(senha, usuario.SenhaHash))
            return ResultadoAuth.Falha("E-mail ou senha inválidos.");

        return ResultadoAuth.Ok(new UsuarioAutenticadoDto(usuario.Id, usuario.Nome, usuario.Email));
    }

    public static string GerarHashSenha(string senha)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(16);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(senha, salt, 100_000, HashAlgorithmName.SHA256, 32);
        return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public static bool VerificarSenha(string senha, string senhaHash)
    {
        var partes = senhaHash.Split('.');
        if (partes.Length != 2) return false;

        try
        {
            byte[] salt = Convert.FromBase64String(partes[0]);
            byte[] hashEsperado = Convert.FromBase64String(partes[1]);
            byte[] hashAtual = Rfc2898DeriveBytes.Pbkdf2(senha, salt, 100_000, HashAlgorithmName.SHA256, 32);
            return CryptographicOperations.FixedTimeEquals(hashAtual, hashEsperado);
        }
        catch
        {
            return false;
        }
    }
}
