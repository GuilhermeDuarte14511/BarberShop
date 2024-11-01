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
            ILogService logService
        ) : base(logService)
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
            await LogAsync("INFO", nameof(AgendamentoController), "Index accessed", "Listando agendamentos");
            var agendamentos = await _agendamentoRepository.GetAllAsync();
            return View(agendamentos);
        }

        public async Task<IActionResult> Details(int id)
        {
            await LogAsync("INFO", nameof(Details), "Detalhes do agendamento acessado", $"ID do Agendamento: {id}");
            var agendamento = await _agendamentoRepository.GetByIdAsync(id);
            if (agendamento == null)
            {
                await LogAsync("WARNING", nameof(Details), "Agendamento não encontrado", $"ID: {id}");
                return NotFound();
            }
            return View(agendamento);
        }

        public IActionResult Create()
        {
            await LogAsync("INFO", nameof(Create), "Tela de criação de agendamento acessada", "");
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
                await LogAsync("INFO", nameof(Create), "Agendamento criado com sucesso", $"ID do Agendamento: {agendamento.AgendamentoId}");
                return RedirectToAction(nameof(Index));
            }
            await LogAsync("WARNING", nameof(Create), "Erro de validação ao criar agendamento", "Dados do ModelState inválidos");
            return View(agendamento);
        }

        public async Task<IActionResult> Edit(int id)
        {
            await LogAsync("INFO", nameof(Edit), "Tela de edição de agendamento acessada", $"ID do Agendamento: {id}");
            var agendamento = await _agendamentoRepository.GetByIdAsync(id);
            if (agendamento == null)
            {
                await LogAsync("WARNING", nameof(Edit), "Agendamento não encontrado para edição", $"ID: {id}");
                return NotFound();
            }

            ViewBag.Clientes = _clienteRepository.GetAllAsync();
            ViewBag.Barbeiros = _barbeiroRepository.GetAllAsync();
            return View(agendamento);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Agendamento agendamento)
        {
            if (id != agendamento.AgendamentoId)
            {
                await LogAsync("ERROR", nameof(Edit), "ID de agendamento inconsistente ao editar", $"ID fornecido: {id}, ID do agendamento: {agendamento.AgendamentoId}");
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                await _agendamentoRepository.UpdateAsync(agendamento);
                await LogAsync("INFO", nameof(Edit), "Agendamento editado com sucesso", $"ID do Agendamento: {agendamento.AgendamentoId}");
                return RedirectToAction(nameof(Index));
            }

            await LogAsync("WARNING", nameof(Edit), "Erro de validação ao editar agendamento", "Dados do ModelState inválidos");
            return View(agendamento);
        }

        public async Task<IActionResult> Delete(int id)
        {
            await LogAsync("INFO", nameof(Delete), "Tela de exclusão de agendamento acessada", $"ID do Agendamento: {id}");
            var agendamento = await _agendamentoRepository.GetByIdAsync(id);
            if (agendamento == null)
            {
                await LogAsync("WARNING", nameof(Delete), "Agendamento não encontrado para exclusão", $"ID: {id}");
                return NotFound();
            }
            return View(agendamento);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _agendamentoRepository.DeleteAsync(id);
            await LogAsync("INFO", nameof(DeleteConfirmed), "Agendamento excluído com sucesso", $"ID do Agendamento: {id}");
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Historico()
        {
            var clienteId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            await LogAsync("INFO", nameof(Historico), "Acessado histórico de agendamentos do cliente", $"ID do Cliente: {clienteId}");
            var agendamentos = await _clienteService.ObterHistoricoAgendamentosAsync(clienteId);
            return View("HistoricoAgendamentos", agendamentos);
        }

        public async Task<IActionResult> ObterHorariosDisponiveis(int barbeiroId, int duracaoTotal)
        {
            await LogAsync("INFO", nameof(ObterHorariosDisponiveis), "Solicitação de horários disponíveis", $"ID do Barbeiro: {barbeiroId}, Duração Total: {duracaoTotal}");

            if (duracaoTotal <= 0)
            {
                await LogAsync("ERROR", nameof(ObterHorariosDisponiveis), "Duração inválida para horários disponíveis", $"Duração fornecida: {duracaoTotal}");
                return BadRequest("A duração dos serviços é inválida.");
            }

            var horariosDisponiveis = await _agendamentoService.ObterHorariosDisponiveisAsync(barbeiroId, DateTime.Now, duracaoTotal);
            return Json(horariosDisponiveis);
        }

        public async Task<IActionResult> ResumoAgendamento(int barbeiroId, DateTime dataHora, string servicoIds)
        {
            await LogAsync("INFO", nameof(ResumoAgendamento), "Resumo de agendamento acessado", $"ID do Barbeiro: {barbeiroId}, DataHora: {dataHora}, Serviços: {servicoIds}");

            var barbeiro = await _barbeiroService.ObterBarbeiroPorIdAsync(barbeiroId);
            if (barbeiro == null)
            {
                await LogAsync("WARNING", nameof(ResumoAgendamento), "Barbeiro não encontrado para o resumo do agendamento", $"ID do Barbeiro: {barbeiroId}");
                return NotFound("Barbeiro não encontrado.");
            }

            var servicoIdList = servicoIds.Split(',').Select(int.Parse).ToList();
            var servicos = await _servicoRepository.ObterServicosPorIdsAsync(servicoIdList);
            var precoTotal = servicos.Sum(s => s.Preco);

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
            await LogAsync("INFO", nameof(ConfirmarAgendamento), "Iniciando confirmação do agendamento", $"ID do Barbeiro: {barbeiroId}, DataHora: {dataHora}, Serviços: {servicoIds}");

            try
            {
                var clienteId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

                if (string.IsNullOrEmpty(servicoIds))
                {
                    await LogAsync("WARNING", nameof(ConfirmarAgendamento), "Nenhum serviço selecionado", $"ID do Cliente: {clienteId}");
                    return Json(new { success = false, message = "Nenhum serviço selecionado." });
                }

                var servicoIdList = servicoIds.Split(',').Select(int.Parse).ToList();
                var servicos = await _servicoRepository.ObterServicosPorIdsAsync(servicoIdList);
                var precoTotal = servicos.Sum(s => s.Preco);
                var duracaoTotal = servicos.Sum(s => s.Duracao);

                var horarioDisponivel = await _barbeiroService.VerificarDisponibilidadeHorarioAsync(barbeiroId, dataHora, duracaoTotal);
                if (!horarioDisponivel)
                {
                    await LogAsync("WARNING", nameof(ConfirmarAgendamento), "Horário indisponível", $"ID do Barbeiro: {barbeiroId}, DataHora: {dataHora}");
                    return Json(new { success = false, message = "O horário selecionado não está mais disponível. Por favor, escolha outro horário." });
                }

                var cliente = await _clienteRepository.GetByIdAsync(clienteId);
                var barbeiro = await _barbeiroRepository.GetByIdAsync(barbeiroId);

                if (cliente == null || barbeiro == null)
                {
                    await LogAsync("ERROR", nameof(ConfirmarAgendamento), "Erro ao obter cliente ou barbeiro", $"ID do Cliente: {clienteId}, ID do Barbeiro: {barbeiroId}");
                    return Json(new { success = false, message = "Erro ao obter informações do cliente ou barbeiro." });
                }

                var agendamentoId = await _agendamentoService.CriarAgendamentoAsync(
                    barbeiroId, dataHora, clienteId, servicoIdList, formaPagamento, (decimal)precoTotal, statusPagamento, paymentId);

                await LogAsync("INFO", nameof(ConfirmarAgendamento), "Agendamento confirmado com sucesso", $"ID do Agendamento: {agendamentoId}");

                return Json(new { success = true, message = "Agendamento confirmado com sucesso!", agendamentoId = agendamentoId });
            }
            catch (Exception ex)
            {
                await LogAsync("ERROR", nameof(ConfirmarAgendamento), $"Erro ao confirmar agendamento: {ex.Message}", $"ID do Barbeiro: {barbeiroId}, DataHora: {dataHora}, Serviços: {servicoIds}");
                return Json(new { success = false, message = "Ocorreu um erro ao confirmar o agendamento. Tente novamente." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AtualizarStatusPagamento(int agendamentoId, StatusPagamento statusPagamento, string paymentId)
        {
            await LogAsync("INFO", nameof(AtualizarStatusPagamento), "Iniciando atualização de status de pagamento", $"ID do Agendamento: {agendamentoId}, Status: {statusPagamento}, PaymentId: {paymentId}");

            try
            {
                var agendamento = await _agendamentoRepository.GetByIdAsync(agendamentoId);
                if (agendamento == null)
                {
                    await LogAsync("WARNING", nameof(AtualizarStatusPagamento), "Agendamento não encontrado para atualizar status de pagamento", $"ID do Agendamento: {agendamentoId}");
                    return Json(new { success = false, message = "Agendamento não encontrado." });
                }

                agendamento.StatusPagamento = statusPagamento;
                agendamento.PaymentId = paymentId;

                await _agendamentoRepository.AtualizarStatusPagamentoAsync(agendamentoId, statusPagamento, paymentId);
                await LogAsync("INFO", nameof(AtualizarStatusPagamento), "Status de pagamento atualizado com sucesso", $"ID do Agendamento: {agendamentoId}");

                return Json(new { success = true, message = "Status de pagamento atualizado com sucesso!" });
            }
            catch (Exception ex)
            {
                await LogAsync("ERROR", nameof(AtualizarStatusPagamento), $"Erro ao atualizar status de pagamento: {ex.Message}", $"ID do Agendamento: {agendamentoId}");
                return Json(new { success = false, message = "Ocorreu um erro ao atualizar o status do pagamento. Tente novamente." });
            }
        }
    }
}
