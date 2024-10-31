using BarberShop.Application.DTOs;
using BarberShop.Application.Interfaces;
using BarberShop.Application.Services;
using BarberShop.Controllers;
using BarberShop.Domain.Entities;
using BarberShop.Domain.Interfaces;
using BarberShop.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BarberShopMVC.Controllers
{
    public class AgendamentoController : BaseController
    {
        private readonly IAgendamentoRepository _agendamentoRepository;
        private readonly IClienteRepository _clienteRepository;
        private readonly IBarbeiroRepository _barbeiroRepository;
        private readonly IAgendamentoService _agendamentoService;
        private readonly IBarbeiroService _barbeiroService;
        private readonly IServicoRepository _servicoRepository;
        private readonly IClienteService _clienteService;
        private readonly IEmailService _emailService;
        private readonly IPaymentService _paymentService;

        public AgendamentoController(
            IAgendamentoRepository agendamentoRepository,
            IClienteRepository clienteRepository,
            IBarbeiroRepository barbeiroRepository,
            IAgendamentoService agendamentoService,
            IBarbeiroService barbeiroService,
            IServicoRepository servicoRepository,
            IClienteService clienteService,
            IEmailService emailService,
            IPaymentService paymentService,
            ILogger<AgendamentoController> logger,
            BarbeariaContext context)
            : base(logger, context)
        {
            _agendamentoRepository = agendamentoRepository;
            _clienteRepository = clienteRepository;
            _barbeiroRepository = barbeiroRepository;
            _agendamentoService = agendamentoService;
            _barbeiroService = barbeiroService;
            _servicoRepository = servicoRepository;
            _clienteService = clienteService;
            _emailService = emailService;
            _paymentService = paymentService;
        }

        public async Task<IActionResult> Index()
        {
            var agendamentos = await _agendamentoRepository.GetAllAsync();
            return View(agendamentos);
        }

        public async Task<IActionResult> Details(int id)
        {
            var agendamento = await _agendamentoRepository.GetByIdAsync(id);
            if (agendamento == null) return NotFound();
            return View(agendamento);
        }

        public IActionResult Create()
        {
            ViewBag.Clientes = _clienteRepository.GetAllAsync();
            ViewBag.Barbeiros = _barbeiroRepository.GetAllAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Agendamento agendamento)
        {
            if (ModelState.IsValid)
            {
                await _agendamentoRepository.AddAsync(agendamento);
                return RedirectToAction(nameof(Index));
            }
            return View(agendamento);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var agendamento = await _agendamentoRepository.GetByIdAsync(id);
            if (agendamento == null) return NotFound();

            ViewBag.Clientes = _clienteRepository.GetAllAsync();
            ViewBag.Barbeiros = _barbeiroRepository.GetAllAsync();
            return View(agendamento);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Agendamento agendamento)
        {
            if (id != agendamento.AgendamentoId) return BadRequest();

            if (ModelState.IsValid)
            {
                await _agendamentoRepository.UpdateAsync(agendamento);
                return RedirectToAction(nameof(Index));
            }
            return View(agendamento);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var agendamento = await _agendamentoRepository.GetByIdAsync(id);
            if (agendamento == null) return NotFound();
            return View(agendamento);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _agendamentoRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Historico()
        {
            var clienteId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var agendamentos = await _clienteService.ObterHistoricoAgendamentosAsync(clienteId);
            return View("HistoricoAgendamentos", agendamentos);
        }

        public async Task<IActionResult> ObterHorariosDisponiveis(int barbeiroId, int duracaoTotal)
        {
            if (duracaoTotal <= 0)
            {
                return BadRequest("A duração dos serviços é inválida.");
            }

            var horariosDisponiveis = await _agendamentoService.ObterHorariosDisponiveisAsync(barbeiroId, DateTime.Now, duracaoTotal);
            return Json(horariosDisponiveis);
        }

        public async Task<IActionResult> ResumoAgendamento(int barbeiroId, DateTime dataHora, string servicoIds)
        {
            var barbeiro = await _barbeiroService.ObterBarbeiroPorIdAsync(barbeiroId);
            if (barbeiro == null) return NotFound("Barbeiro não encontrado.");

            var servicoIdList = servicoIds.Split(',').Select(int.Parse).ToList();
            var servicos = await _servicoRepository.ObterServicosPorIdsAsync(servicoIdList);
            var precoTotal = servicos.Sum(s => s.Preco);

            var clienteNome = User.FindFirst(ClaimTypes.Name)?.Value;
            var clienteEmail = User.FindFirst(ClaimTypes.Email)?.Value;

            ViewData["ClienteNome"] = clienteNome;
            ViewData["ClienteEmail"] = clienteEmail;

            var resumoAgendamentoDTO = new ResumoAgendamentoDTO
            {
                NomeBarbeiro = barbeiro.Nome,
                BarbeiroId = barbeiro.BarbeiroId,
                DataHora = dataHora,
                ServicosSelecionados = servicos.Select(s => new ServicoDTO
                {
                    ServicoId = s.ServicoId,
                    Nome = s.Nome,
                    Preco = (decimal)s.Preco
                }).ToList(),
                PrecoTotal = (decimal)precoTotal,
                FormaPagamento = ""
            };

            return View("ResumoAgendamento", resumoAgendamentoDTO);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmarAgendamento(int barbeiroId, DateTime dataHora, string servicoIds, string formaPagamento, string paymentMethodId = null, string paymentId = null)
        {
            try
            {
                var clienteId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

                if (string.IsNullOrEmpty(servicoIds))
                {
                    TempData["MensagemErro"] = "Nenhum serviço selecionado.";
                    return RedirectToAction("MenuPrincipal", "Cliente");
                }

                var servicoIdList = servicoIds.Split(',').Select(int.Parse).ToList();
                var servicos = await _servicoRepository.ObterServicosPorIdsAsync(servicoIdList);

                decimal precoTotal = (decimal)servicos.Sum(s => s.Preco);

                var cliente = await _clienteRepository.GetByIdAsync(clienteId);
                var barbeiro = await _barbeiroRepository.GetByIdAsync(barbeiroId);

                if (cliente == null || barbeiro == null)
                {
                    TempData["MensagemErro"] = "Erro ao obter informações do cliente ou barbeiro.";
                    return RedirectToAction("MenuPrincipal", "Cliente");
                }

                var duracaoTotal = servicos.Sum(s => s.Duracao);
                var dataHoraFim = dataHora.AddMinutes(duracaoTotal);
                var agendamentosExistentes = await _agendamentoRepository
                    .ObterAgendamentosPorBarbeiroEData(barbeiroId, dataHora, dataHoraFim);

                if (agendamentosExistentes.Any())
                {
                    TempData["MensagemErro"] = "O horário selecionado não está mais disponível. Por favor, escolha outro horário.";
                    return RedirectToAction("MenuPrincipal", "Cliente");
                }

                var agendamentoId = await _agendamentoService.CriarAgendamentoAsync(
                    barbeiroId,
                    dataHora,
                    clienteId,
                    servicoIdList,
                    formaPagamento,
                    precoTotal,
                    paymentId
                );

                var agendamento = await _agendamentoRepository.GetByIdAsync(agendamentoId);
                agendamento.Status = StatusAgendamento.Confirmado;
                agendamento.StatusPagamento = formaPagamento == "pix" && string.IsNullOrEmpty(paymentId) ? StatusPagamento.Pendente : StatusPagamento.Aprovado;
                await _agendamentoRepository.UpdateAsync(agendamento);

                var googleCalendarLink = _emailService.GerarLinkGoogleCalendar(
                    "Agendamento na Barbearia CG DREAMS",
                    dataHora,
                    dataHoraFim,
                    $"Agendamento com o barbeiro {barbeiro.Nome} para os serviços: {string.Join(", ", servicos.Select(s => s.Nome))}",
                    "Endereço da Barbearia"
                );

                await _emailService.EnviarEmailAgendamentoAsync(
                    cliente.Email,
                    cliente.Nome,
                    "Confirmação de Agendamento - Barbearia CG DREAMS",
                    "Seu agendamento foi confirmado com sucesso!",
                    barbeiro.Nome,
                    dataHora,
                    dataHoraFim,
                    precoTotal,
                    formaPagamento,
                    googleCalendarLink
                );

                var servicoNomes = servicos.Select(s => s.Nome).ToList();
                await _emailService.EnviarEmailNotificacaoBarbeiroAsync(
                    barbeiro.Email,
                    barbeiro.Nome,
                    cliente.Nome,
                    servicoNomes,
                    dataHora,
                    dataHoraFim,
                    precoTotal,
                    formaPagamento
                );

                TempData["MensagemSucesso"] = "Agendamento confirmado com sucesso!";

                if (formaPagamento == "pix" && paymentId == null)
                {
                    var pixQrCode = "Gerar QR code aqui";
                    return Ok(new { qrCode = pixQrCode });
                }
            }
            catch (Exception ex)
            {
                TempData["MensagemErro"] = "Ocorreu um erro ao confirmar o agendamento. Tente novamente.";
                await SaveLogAsync("ERROR", "AgendamentoController", $"Erro ao confirmar agendamento: {ex.Message}", null);
                return StatusCode(500, "Erro ao processar pagamento.");
            }

            return RedirectToAction("MenuPrincipal", "Cliente");
        }
    }
}
