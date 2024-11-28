using Microsoft.AspNetCore.Mvc;
using BarberShop.Application.Interfaces;
using BarberShop.Application.DTOs;
using BarberShop.Application.Services;

namespace BarberShop.Controllers
{
    public class NotificacaoController : BaseController
    {
        private readonly INotificacaoService _notificacaoService;

        public NotificacaoController(ILogService logService, INotificacaoService notificacaoService)
            : base(logService)
        {
            _notificacaoService = notificacaoService;
        }

        [HttpGet]
        public IActionResult ObterNotificacoes()
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out var id))
                    return Unauthorized("Usuário não identificado ou ID inválido.");

                // Busca somente notificações não lidas
                var notificacoes = _notificacaoService
                    .ObterPorUsuario(id)
                    .Where(n => !n.Lida) // Filtrar somente notificações não lidas
                    .ToList();

                return Json(notificacoes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao obter notificações.", detalhe = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult ObterNotificacoesPorDia()
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out var id))
                    return Unauthorized("Usuário não identificado ou ID inválido.");

                var notificacoesAgrupadas = _notificacaoService.ObterNotificacoesAgrupadasPorDia(id);

                return Json(notificacoesAgrupadas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao obter notificações.", detalhe = ex.Message });
            }
        }



        [HttpPost]
        public IActionResult MarcarTodasComoLidas()
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out var id))
                    return Unauthorized("Usuário não identificado ou ID inválido.");

                _notificacaoService.MarcarTodasComoLidas(id);

                return Ok(new { message = "Notificações marcadas como lidas com sucesso." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao marcar notificações como lidas.", detalhe = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CriarNotificacao([FromBody] NotificacaoDTO notificacao)
        {
            try
            {
                if (notificacao == null || notificacao.UsuarioId <= 0 || notificacao.BarbeariaId <= 0 || string.IsNullOrEmpty(notificacao.Mensagem))
                    return BadRequest(new { message = "Dados da notificação inválidos." });

                // Cria a notificação usando o serviço
                _notificacaoService.CriarNotificacao(notificacao);

                await LogAsync(
                    logLevel: "Info",
                    source: nameof(NotificacaoController),
                    message: "Nova notificação criada",
                    data: $"Notificação para usuário ID: {notificacao.UsuarioId}"
                );

                return Ok(new { message = "Notificação criada com sucesso." });
            }
            catch (Exception ex)
            {
                await LogAsync(
                    logLevel: "Error",
                    source: nameof(NotificacaoController),
                    message: "Erro ao criar notificação",
                    data: ex.ToString()
                );

                return StatusCode(500, new { message = "Erro ao criar notificação.", detalhe = ex.Message });
            }
        }
    }
}
