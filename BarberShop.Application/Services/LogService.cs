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
                var usuarioIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
                var usuarioId = usuarioIdClaim != null ? usuarioIdClaim.Value : "Desconhecido";

                // Validação de claims
                if (usuarioId == "Desconhecido")
                {
                    _logger.LogWarning("Usuário não autenticado ao registrar log.");
                }

                logLevel = string.IsNullOrWhiteSpace(logLevel) ? "Information" : logLevel;
                data = data ?? DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss");

                var formattedMessage = $"Usuario Id {usuarioId}, {message}";

                var logEntry = new Log
                {
                    LogLevel = logLevel,
                    Source = source,
                    Message = formattedMessage,
                    Data = data,
                    ResourceID = resourceId ?? "N/A",
                    LogDateTime = DateTime.UtcNow
                };

                _context.Logs.Add(logEntry);
                await _context.SaveChangesAsync();
                _logger.LogDebug($"Log salvo com sucesso: {formattedMessage}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao salvar log: {ex.Message}");
            }
        }

    }
}
