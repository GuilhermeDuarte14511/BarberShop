using BarberShop.Application.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

public class BaseController : Controller
{
    protected readonly ILogService _logService; // Mudado para protected

    public BaseController(ILogService logService)
    {
        _logService = logService;
    }

    /// <summary>
    /// Registra logs assíncronos no sistema.
    /// </summary>
    /// <param name="logLevel">Nível do log (Information, Warning, Error).</param>
    /// <param name="source">Fonte do log (ex: nome do controller ou serviço).</param>
    /// <param name="message">Mensagem descritiva.</param>
    /// <param name="data">Dados adicionais.</param>
    /// <param name="resourceId">ID do recurso relacionado (opcional).</param>
    protected async Task LogAsync(string logLevel, string source, string message, string data, string resourceId = null)
    {
        await _logService.SaveLogAsync(logLevel, source, message, data, resourceId);
    }

    /// <summary>
    /// Gera uma senha aleatória segura.
    /// </summary>
    /// <param name="tamanho">Tamanho da senha gerada (padrão: 8 caracteres).</param>
    /// <returns>Senha gerada como string.</returns>
    [NonAction]
    public string GerarSenhaAleatoria(int tamanho = 8)
    {
        const string caracteres = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var senha = new StringBuilder();

        for (int i = 0; i < tamanho; i++)
        {
            int index = RandomNumberGenerator.GetInt32(caracteres.Length);
            senha.Append(caracteres[index]);
        }

        return senha.ToString();
    }

    /// <summary>
    /// Obtém o ID do barbeiro logado com base no claim "BarbeiroId".
    /// </summary>
    /// <returns>ID do barbeiro logado (int).</returns>
    [NonAction]
    public int ObterBarbeiroIdLogado()
    {
        var barbeiroIdClaim = User.FindFirst("BarbeiroId")?.Value;
        return int.TryParse(barbeiroIdClaim, out var barbeiroId) ? barbeiroId : 0;
    }

    /// <summary>
    /// Obtém o ID do usuário logado com base no claim "NameIdentifier".
    /// </summary>
    /// <returns>ID do usuário logado (int).</returns>
    [NonAction]
    public int ObterUsuarioIdLogado()
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(usuarioIdClaim, out var usuarioId) ? usuarioId : 0;
    }

    /// <summary>
    /// Retorna todos os claims do usuário logado como dicionário.
    /// </summary>
    /// <returns>Dicionário contendo chave (tipo do claim) e valor.</returns>
    [NonAction]
    public Dictionary<string, string> ObterTodosClaims()
    {
        return User.Claims.ToDictionary(c => c.Type, c => c.Value);
    }
}
