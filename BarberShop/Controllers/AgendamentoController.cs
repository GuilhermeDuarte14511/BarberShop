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
        private readonly IEmailService _emailService; // Injetando o IEmailService

        public AgendamentoController(
            IAgendamentoRepository agendamentoRepository,
            IClienteRepository clienteRepository,
            IBarbeiroRepository barbeiroRepository,
            IAgendamentoService agendamentoService,
            IBarbeiroService barbeiroService,
            IServicoRepository servicoRepository,
            IClienteService clienteService,
            IEmailService emailService) // Recebendo via injeção de dependência
        {
            _agendamentoRepository = agendamentoRepository;
            _clienteRepository = clienteRepository;
            _barbeiroRepository = barbeiroRepository;
            _agendamentoService = agendamentoService;
            _barbeiroService = barbeiroService;
            _servicoRepository = servicoRepository;
            _clienteService = clienteService;
            _emailService = emailService; // Atribuindo ao campo privado
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
                PrecoTotal = (decimal)precoTotal
            };

            return View("ResumoAgendamento", resumoAgendamentoDTO);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmarAgendamento(int barbeiroId, DateTime dataHora, string servicoIds)
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

                // Criar o agendamento e obter o ID
                var agendamentoId = await _agendamentoService.CriarAgendamentoAsync(barbeiroId, dataHora, clienteId, servicoIdList);

                // Obter informações adicionais
                var cliente = await _clienteRepository.GetByIdAsync(clienteId);
                var barbeiro = await _barbeiroRepository.GetByIdAsync(barbeiroId);

                if (cliente == null || barbeiro == null)
                {
                    TempData["MensagemErro"] = "Erro ao obter informações do cliente ou barbeiro.";
                    return RedirectToAction("MenuPrincipal", "Cliente");
                }

                var servicos = await _servicoRepository.ObterServicosPorIdsAsync(servicoIdList);
                var precoTotal = servicos.Sum(s => s.Preco);

                // Calcular a data e hora de fim do agendamento
                var duracaoTotal = servicos.Sum(s => s.Duracao);
                var dataHoraFim = dataHora.AddMinutes(duracaoTotal);

                // Gerar link para o Google Calendar
                var tituloEvento = "Agendamento na Barbearia CG DREAMS";
                var descricaoEvento = $"Agendamento com o barbeiro {barbeiro.Nome} para os serviços: {string.Join(", ", servicos.Select(s => s.Nome))}";
                var localEvento = "Endereço da Barbearia";

                var googleCalendarLink = _emailService.GerarLinkGoogleCalendar(tituloEvento, dataHora, dataHoraFim, descricaoEvento, localEvento);

                // Enviar e-mail para o cliente
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
                    googleCalendarLink
                );

                // Enviar e-mail para o barbeiro
                var assuntoBarbeiro = "Novo Agendamento - Barbearia CG DREAMS";
                var conteudoBarbeiro = $"Você tem um novo agendamento com o cliente {cliente.Nome}.";

                await _emailService.EnviarEmailAgendamentoAsync(
                    barbeiro.Email,
                    barbeiro.Nome,
                    assuntoBarbeiro,
                    conteudoBarbeiro,
                    cliente.Nome,
                    dataHora,
                    dataHoraFim,
                    (decimal)precoTotal
                );

                TempData["MensagemSucesso"] = "Agendamento confirmado com sucesso!";
            }
            catch (Exception ex)
            {
                TempData["MensagemErro"] = "Ocorreu um erro ao confirmar o agendamento. Tente novamente.";
                // Aqui você pode registrar o erro em um log para análise
                Console.WriteLine($"Erro ao confirmar agendamento: {ex.Message}");
            }

            return RedirectToAction("MenuPrincipal", "Cliente");
        }
    }
}
