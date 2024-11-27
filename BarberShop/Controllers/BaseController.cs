using BarberShop.Application.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

public class BaseController : Controller
{
    protected readonly ILogService _logService; // Mudado para protected

    public BaseController(ILogService logService)
    {
        _logService = logService;
    }

    protected async Task LogAsync(string logLevel, string source, string message, string data, string resourceId = null)
    {
        await _logService.SaveLogAsync(logLevel, source, message, data, resourceId);
    }

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

    // Função para obter o ID do barbeiro logado a partir do claim
    public int ObterBarbeiroIdLogado()
    {
        var barbeiroIdClaim = User.FindFirst("BarbeiroId")?.Value;
        return int.Parse(barbeiroIdClaim ?? "0");
    }

    public Dictionary<string, string> ObterTodosClaims()
    {
        return User.Claims.ToDictionary(c => c.Type, c => c.Value);
    }

}
