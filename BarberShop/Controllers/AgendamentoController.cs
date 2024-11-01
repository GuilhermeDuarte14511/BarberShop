using BarberShop.Application.DTOs;
using BarberShop.Application.Interfaces;
using BarberShop.Application.Services;
using BarberShop.Domain.Entities;
using BarberShop.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BarberShopMVC.Controllers
{
    public class AgendamentoController : Controller
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
            IPaymentService paymentService)
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

        // Métodos relacionados aos agendamentos

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

            // Obtém o cliente autenticado
            var clienteId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var cliente = await _clienteRepository.GetByIdAsync(clienteId);

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

            // Armazenar o nome e o email do cliente na ViewData
            if (cliente != null)
            {
                ViewData["ClienteNome"] = cliente.Nome;
                ViewData["ClienteEmail"] = cliente.Email;
            }

            return View("ResumoAgendamento", resumoAgendamentoDTO);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmarAgendamento(int barbeiroId, DateTime dataHora, string servicoIds, string formaPagamento, StatusPagamento statusPagamento = StatusPagamento.Pendente, string paymentId = null)
        {
            try
            {
                var clienteId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

                if (string.IsNullOrEmpty(servicoIds))
                {
                    return Json(new { success = false, message = "Nenhum serviço selecionado." });
                }

                var servicoIdList = servicoIds.Split(',').Select(int.Parse).ToList();
                var servicos = await _servicoRepository.ObterServicosPorIdsAsync(servicoIdList);
                var precoTotal = servicos.Sum(s => s.Preco);
                var duracaoTotal = servicos.Sum(s => s.Duracao);

                // Verificar disponibilidade de horário
                var horarioDisponivel = await _barbeiroService.VerificarDisponibilidadeHorarioAsync(barbeiroId, dataHora, duracaoTotal);
                if (!horarioDisponivel)
                {
                    return Json(new { success = false, message = "O horário selecionado não está mais disponível. Por favor, escolha outro horário." });
                }

                var cliente = await _clienteRepository.GetByIdAsync(clienteId);
                var barbeiro = await _barbeiroRepository.GetByIdAsync(barbeiroId);

                if (cliente == null || barbeiro == null)
                {
                    return Json(new { success = false, message = "Erro ao obter informações do cliente ou barbeiro." });
                }

                // Criar o agendamento com o status de pagamento e ID de pagamento fornecidos
                var agendamentoId = await _agendamentoService.CriarAgendamentoAsync(
                    barbeiroId, dataHora, clienteId, servicoIdList, formaPagamento, (decimal)precoTotal, statusPagamento, paymentId);

                // Configurar informações para envio de email
                var dataHoraFim = dataHora.AddMinutes(duracaoTotal);
                var tituloEvento = "Agendamento na Barbearia CG DREAMS";
                var descricaoEvento = $"Agendamento com o barbeiro {barbeiro.Nome} para os serviços: {string.Join(", ", servicos.Select(s => s.Nome))}";
                var localEvento = "Endereço da Barbearia";
                var googleCalendarLink = _emailService.GerarLinkGoogleCalendar(tituloEvento, dataHora, dataHoraFim, descricaoEvento, localEvento);

                var assuntoCliente = "Confirmação de Agendamento - Barbearia CG DREAMS";
                var conteudoCliente = "Seu agendamento foi confirmado com sucesso!";
                await _emailService.EnviarEmailAgendamentoAsync(
                    cliente.Email,
                    cliente.Nome,
                    assuntoCliente,
                    conteudoCliente,
                    barbeiro.Nome,
                    dataHora,
                    dataHoraFim,
                    (decimal)precoTotal,
                    formaPagamento,
                    googleCalendarLink
                );

                var assuntoBarbeiro = "Novo Agendamento - Barbearia CG DREAMS";
                var servicoNomes = servicos.Select(s => s.Nome).ToList();
                await _emailService.EnviarEmailNotificacaoBarbeiroAsync(
                    barbeiro.Email,
                    barbeiro.Nome,
                    cliente.Nome,
                    servicoNomes,
                    dataHora,
                    dataHoraFim,
                    (decimal)precoTotal,
                    formaPagamento
                );

                // Retorno de sucesso com o ID do agendamento
                return Json(new { success = true, message = "Agendamento confirmado com sucesso!", agendamentoId = agendamentoId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao confirmar agendamento: {ex.Message}");
                return Json(new { success = false, message = "Ocorreu um erro ao confirmar o agendamento. Tente novamente." });
            }
        }


        [HttpPost]
        public async Task<IActionResult> AtualizarStatusPagamento(int agendamentoId, StatusPagamento statusPagamento, string paymentId)
        {
            try
            {
                var agendamento = await _agendamentoRepository.GetByIdAsync(agendamentoId);
                if (agendamento == null)
                {
                    return Json(new { success = false, message = "Agendamento não encontrado." });
                }

                // Atualizar status de pagamento e paymentId
                agendamento.StatusPagamento = statusPagamento;
                agendamento.PaymentId = paymentId;

                // Chamar o método de repositório para persistir as alterações
                await _agendamentoRepository.AtualizarStatusPagamentoAsync(agendamentoId, statusPagamento, paymentId);

                return Json(new { success = true, message = "Status de pagamento atualizado com sucesso!" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao atualizar status de pagamento: {ex.Message}");
                return Json(new { success = false, message = "Ocorreu um erro ao atualizar o status do pagamento. Tente novamente." });
            }
        }




    }
}
