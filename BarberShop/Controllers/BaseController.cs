using BarberShop.Application.Services;
using Microsoft.AspNetCore.Mvc;

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
}
