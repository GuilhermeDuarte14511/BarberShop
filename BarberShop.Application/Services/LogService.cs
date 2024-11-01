using BarberShop.Domain.Entities;
using BarberShop.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace BarberShop.Application.Services
{
    public class LogService : ILogService
    {
        private readonly BarbeariaContext _context;
        private readonly ILogger<LogService> _logger;

        public LogService(BarbeariaContext context, ILogger<LogService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task SaveLogAsync(string logLevel, string source, string message, string data, string resourceId = null)
        {
            _logger.LogDebug("Salvando log no banco de dados...");
            var logEntry = new Log
            {
                LogLevel = logLevel,
                Source = source,
                Message = message,
                Data = data,
                ResourceID = resourceId,
                LogDateTime = DateTime.UtcNow
            };

            _context.Logs.Add(logEntry);
            await _context.SaveChangesAsync();
            _logger.LogDebug("Log salvo com sucesso.");
        }
    }
}
