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
    public class ClienteController : Controller
    {
        private readonly IClienteService _clienteService;
        private readonly IServicoRepository _servicoRepository;
        private readonly IAgendamentoService _agendamentoService; // Serviço de agendamento
        private readonly IBarbeiroService _barbeiroService; // Serviço de barbeiros
        // private readonly IRabbitMQService _rabbitMQService; // RabbitMQ para envio de emails (Comentado)

        public ClienteController(
            IClienteService clienteService,
            IServicoRepository servicoRepository,
            IAgendamentoService agendamentoService,
            IBarbeiroService barbeiroService
        // IRabbitMQService rabbitMQService // Adicionado RabbitMQService (Comentado)
        )
        {
            _clienteService = clienteService;
            _servicoRepository = servicoRepository;
            _agendamentoService = agendamentoService;
            _barbeiroService = barbeiroService;
            // _rabbitMQService = rabbitMQService; (Comentado)
        }

        public IActionResult MenuPrincipal()
        {
            return View();
        }

        public async Task<IActionResult> Historico()
        {
            var clienteId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var agendamentos = await _clienteService.ObterHistoricoAgendamentosAsync(clienteId);
            return View("HistoricoAgendamentos", agendamentos);
        }

        public async Task<IActionResult> SolicitarServico()
        {
            var servicos = await _servicoRepository.GetAllAsync();
            return View("SolicitarServico", servicos);
        }

        public async Task<IActionResult> EscolherBarbeiro(int duracaoTotal, string servicoIds)
        {
            if (duracaoTotal <= 0)
            {
                return BadRequest("A duração dos serviços é inválida.");
            }

            var barbeiros = await _barbeiroService.ObterTodosBarbeirosAsync();
            ViewData["DuracaoTotal"] = duracaoTotal;
            ViewData["ServicoIds"] = servicoIds; // Passar os IDs dos serviços para a View
            return View("EscolherBarbeiro", barbeiros);
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

        // Método que exibe o resumo do agendamento
        public async Task<IActionResult> ResumoAgendamento(int barbeiroId, DateTime dataHora, string servicoIds)
        {
            var barbeiro = await _barbeiroService.ObterBarbeiroPorIdAsync(barbeiroId);
            if (barbeiro == null) return NotFound("Barbeiro não encontrado.");

            // Converter a string de servicoIds para List<int>
            List<int> servicoIdList = servicoIds.Split(',').Select(int.Parse).ToList();

            var servicos = await _servicoRepository.ObterServicosPorIdsAsync(servicoIdList);
            var precoTotal = servicos.Sum(s => s.Preco);

            var resumoAgendamentoDTO = new ResumoAgendamentoDTO
            {
                NomeBarbeiro = barbeiro.Nome,
                BarbeiroId = barbeiro.BarbeiroId,
                DataHora = dataHora,
                ServicosSelecionados = servicos.Select(s => new ServicoDTO
                {
                    ServicoId = s.ServicoId, // Adiciona o ServicoId ao DTO
                    Nome = s.Nome,
                    Preco = (decimal)s.Preco // Conversão explícita de float para decimal
                }).ToList(),
                PrecoTotal = (decimal)precoTotal // Conversão explícita de float para decimal
            };

            return View("ResumoAgendamento", resumoAgendamentoDTO);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmarAgendamento(int barbeiroId, DateTime dataHora, string servicoIds)
        {
            try
            {
                var clienteId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

                // Verifica se há serviços selecionados
                if (string.IsNullOrEmpty(servicoIds))
                {
                    TempData["MensagemErro"] = "Nenhum serviço selecionado.";
                    return RedirectToAction("MenuPrincipal");
                }

                List<int> servicoIdList = servicoIds.Split(',').Select(int.Parse).ToList();

                // Cria o agendamento e retorna o ID do agendamento recém-criado, passando o clienteId
                var agendamentoId = await _agendamentoService.CriarAgendamentoAsync(barbeiroId, dataHora, clienteId, servicoIdList);

                // Código relacionado ao RabbitMQ comentado
                /*
                var agendamentoMessage = new
                {
                    ClienteId = clienteId,
                    BarbeiroId = barbeiroId,
                    DataHora = dataHora,
                    ServicoIds = servicoIdList,
                    AgendamentoId = agendamentoId
                };

                // Serializa a mensagem para envio
                var mensagemJson = Newtonsoft.Json.JsonConvert.SerializeObject(agendamentoMessage);

                // Enviar mensagem para a fila do RabbitMQ
                _rabbitMQService.EnviarParaFila(mensagemJson);
                */

                // Exibir uma mensagem de confirmação e redirecionar para o menu principal
                TempData["MensagemSucesso"] = "Agendamento confirmado com sucesso!";
            }
            catch (Exception ex)
            {
                // Caso ocorra algum erro, exibe uma mensagem apropriada e loga o erro (se necessário)
                TempData["MensagemErro"] = "Ocorreu um erro ao confirmar o agendamento. Tente novamente.";
                // Logar o erro se necessário
                // _logger.LogError(ex, "Erro ao confirmar agendamento");
            }

            return RedirectToAction("MenuPrincipal");
        }

        // Exibe todos os clientes
        public async Task<IActionResult> Index()
        {
            var clientes = await _clienteService.ObterTodosClientesAsync();
            return View(clientes);
        }

        // Exibe detalhes de um cliente específico
        public async Task<IActionResult> Details(int id)
        {
            var cliente = await _clienteService.ObterClientePorIdAsync(id);
            if (cliente == null) return NotFound();
            return View(cliente);
        }

        // Exibe a view de criação de cliente
        public IActionResult Create()
        {
            return View();
        }

        // Cria um novo cliente
        [HttpPost]
        public async Task<IActionResult> Create(ClienteDTO clienteDto) // Mudança para usar DTO
        {
            if (ModelState.IsValid)
            {
                var cliente = new Cliente
                {
                    Nome = clienteDto.Nome,
                    Email = clienteDto.Email,
                    Telefone = clienteDto.Telefone,
                };

                await _clienteService.AdicionarClienteAsync(cliente);
                return RedirectToAction(nameof(Index));
            }
            return View(clienteDto);
        }

        // Exibe a view de edição de cliente
        public async Task<IActionResult> Edit(int id)
        {
            var cliente = await _clienteService.ObterClientePorIdAsync(id);
            if (cliente == null) return NotFound();

            var clienteDto = new ClienteDTO
            {
                Nome = cliente.Nome,
                Email = cliente.Email,
                Telefone = cliente.Telefone,
            };

            return View(clienteDto);
        }

        // Atualiza um cliente existente
        [HttpPost]
        public async Task<IActionResult> Edit(int id, ClienteDTO clienteDto) // Mudança para usar DTO
        {
            if (id != clienteDto.ClienteId) return BadRequest();

            if (ModelState.IsValid)
            {
                var cliente = new Cliente
                {
                    ClienteId = clienteDto.ClienteId,
                    Nome = clienteDto.Nome,
                    Email = clienteDto.Email,
                    Telefone = clienteDto.Telefone,
                };

                await _clienteService.AtualizarClienteAsync(cliente);
                return RedirectToAction(nameof(Index));
            }
            return View(clienteDto);
        }

        // Exibe a view de exclusão de cliente
        public async Task<IActionResult> Delete(int id)
        {
            var cliente = await _clienteService.ObterClientePorIdAsync(id);
            if (cliente == null) return NotFound();
            return View(cliente);
        }

        // Exclui um cliente
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _clienteService.DeletarClienteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
