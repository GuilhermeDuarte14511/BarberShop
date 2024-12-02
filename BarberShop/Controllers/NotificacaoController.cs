using Microsoft.AspNetCore.Mvc;
using BarberShop.Application.Interfaces;
using BarberShop.Application.DTOs;
using BarberShop.Application.Services;
using WebPush;

namespace BarberShop.Controllers
{
    public class NotificacaoController : BaseController
    {
        private readonly INotificacaoService _notificacaoService;
        private readonly IPushSubscriptionService _pushSubscriptionService;

        public NotificacaoController(ILogService logService, INotificacaoService notificacaoService, IPushSubscriptionService pushSubscriptionService)
            : base(logService)
        {
            _notificacaoService = notificacaoService;
            _pushSubscriptionService = pushSubscriptionService;
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

                // Buscar inscrições do usuário (PushSubscriptions)
                var subscriptions = _pushSubscriptionService.ObterInscricoesPorUsuario(notificacao.UsuarioId.Value);

                // Enviar notificações push para os navegadores registrados
                foreach (var subscription in subscriptions)
                {
                    try
                    {
                        EnviarNotificacaoPush(subscription, notificacao);
                    }
                    catch (Exception ex)
                    {
                        await LogAsync(
                            logLevel: "Error",
                            source: nameof(NotificacaoController),
                            message: $"Erro ao enviar notificação push para inscrição ID: {subscription.Id}",
                            data: ex.ToString()
                        );
                    }
                }

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


        [HttpPost]
        [Route("RegistrarInscricao")]
        public IActionResult RegistrarInscricao([FromBody] PushSubscriptionDTO subscription)
        {
            try
            {
                if (subscription == null || string.IsNullOrEmpty(subscription.Endpoint))
                    return BadRequest(new { message = "Inscrição inválida." });

                _pushSubscriptionService.SalvarInscricao(subscription);

                return Ok(new { message = "Inscrição registrada com sucesso." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao registrar inscrição.", detalhe = ex.Message });
            }
        }

        private void EnviarNotificacaoPush(PushSubscriptionDTO subscription, NotificacaoDTO notificacao)
        {
            var vapidDetails = new VapidDetails(
                "mailto:gui14511@gmail.com",
                "BFAb29jRZDa3O4RuCPfX_UoYreH3DUecJgfPPk7kwh3Os77M_vN4_2eZEiT1axF4GpywgXa7oV9ucgDZh56OslQ", // Chave pública
                "EC0GojYmK5jEZrnf3XC2XSqPLEDVP90K_oJUwT2gTv8" // Chave privada
            );

            var pushSubscription = new WebPush.PushSubscription(
                subscription.Endpoint,
                subscription.P256dh,
                subscription.Auth
            );

            var payload = new
            {
                title = "Nova Notificação",
                body = notificacao.Mensagem,
                icon = "/favicon.ico", // Ícone opcional
                url = notificacao.Link // Link opcional associado à notificação
            };

            var webPushClient = new WebPushClient();
            webPushClient.SendNotification(pushSubscription, System.Text.Json.JsonSerializer.Serialize(payload), vapidDetails);
        }

        [HttpGet]
        public IActionResult Index()
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out var id))
                    return Unauthorized("Usuário não identificado ou ID inválido.");

                var notificacoes = _notificacaoService.ObterPorUsuario(id);

                // Separar notificações
                ViewData["NotificacoesNaoLidas"] = notificacoes.Where(n => !n.Lida).ToList();
                ViewData["NotificacoesLidas"] = notificacoes.Where(n => n.Lida).ToList();

                return View(notificacoes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao carregar notificações.", detalhe = ex.Message });
            }
        }


    }
}
