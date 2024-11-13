using BarberShop.Domain.Entities;
using BarberShop.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BarberShop.Application.Services
{
    public class LogService : ILogService
    {
        private readonly BarbeariaContext _context;
        private readonly ILogger<LogService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LogService(BarbeariaContext context, ILogger<LogService> logger, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task SaveLogAsync(string logLevel, string source, string message, string data = null, string resourceId = null)
        {
            try
            {
                _logger.LogDebug("Salvando log no banco de dados...");

                // Obtenha o usuarioId dos claims, se estiver disponível
                var usuarioIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
                var usuarioId = usuarioIdClaim != null ? usuarioIdClaim.Value : "Desconhecido";

                // Validações e atribuições de valores padrão
                logLevel = string.IsNullOrWhiteSpace(logLevel) ? "Information" : logLevel;
                source = string.IsNullOrWhiteSpace(source) ? "Source não especificado" : source;
                message = string.IsNullOrWhiteSpace(message) ? "Mensagem não fornecida" : message;
                data = data ?? DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss");
                resourceId = string.IsNullOrWhiteSpace(resourceId) ? "N/A" : resourceId;

                // Adiciona o usuarioId ao início da mensagem
                var formattedMessage = $"Usuario Id {usuarioId}, {message}";

                var logEntry = new Log
                {
                    LogLevel = logLevel,
                    Source = source,
                    Message = formattedMessage,
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
                _logger.LogError($"Erro ao salvar log no banco de dados: {ex.Message}");
                throw new Exception("Erro ao salvar log no banco de dados.");
            }
        }
    }
}
