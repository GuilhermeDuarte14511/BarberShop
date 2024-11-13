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

        public async Task SaveLogAsync(string logLevel, string source, string message, string data = null, string resourceId = null)
        {
            try
            {
                _logger.LogDebug("Salvando log no banco de dados...");

                // Validações e atribuições de valores padrão
                logLevel = string.IsNullOrWhiteSpace(logLevel) ? "Information" : logLevel;
                source = string.IsNullOrWhiteSpace(source) ? "Source não especificado" : source;
                message = string.IsNullOrWhiteSpace(message) ? "Mensagem não fornecida" : message;
                data = data ?? DateTime.UtcNow.ToString("dd/MM/yyyy hh:mm");
                resourceId = string.IsNullOrWhiteSpace(resourceId) ? "N/A" : resourceId;

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
            catch (Exception ex)
            {
                // Log de erro para o logger interno, com detalhes para rastrear o problema.
                _logger.LogError($"Erro ao salvar log no banco de dados: {ex.Message}");
                throw new Exception("Erro ao salvar log no banco de dados.");
            }
        }

    }
}
