using Microsoft.AspNetCore.Mvc;
using BarberShop.Application.Services;
using System.Threading.Tasks;

namespace BarberShopMVC.Controllers
{
    public class BaseController : Controller
    {
        private readonly ILogService _logService;

        public BaseController(ILogService logService)
        {
            _logService = logService;
        }

        protected async Task LogAsync(string logLevel, string source, string message, string data, string resourceId = null)
        {
            await _logService.SaveLogAsync(logLevel, source, message, data, resourceId);
        }
    }
}
