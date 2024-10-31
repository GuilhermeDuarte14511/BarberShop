using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BarberShop.Infrastructure.Data;
using BarberShop.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace BarberShop.Controllers
{
    public class BaseController : Controller
    {
        protected readonly ILogger<BaseController> Logger;
        protected readonly BarbeariaContext Context;

        public BaseController(ILogger<BaseController> logger, BarbeariaContext context)
        {
            Logger = logger;
            Context = context;
        }

        // Método auxiliar para salvar logs no banco de dados
        protected async Task SaveLogAsync(string logLevel, string source, string message, string data, string resourceId = null)
        {
            Logger.LogDebug("Salvando log no banco de dados...");
            var logEntry = new Log
            {
                LogLevel = logLevel,
                Source = source,
                Message = message,
                Data = data,
                ResourceID = resourceId,
                LogDateTime = DateTime.UtcNow
            };

            Context.Logs.Add(logEntry);
            await Context.SaveChangesAsync();
            Logger.LogDebug("Log salvo com sucesso.");
        }
    }
}
