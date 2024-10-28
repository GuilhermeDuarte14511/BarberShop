using BarberShop.Application.DTOs;
using BarberShop.Application.Interfaces;
using BarberShop.Application.Services;
using BarberShop.Domain.Entities;
using BarberShop.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BarberShopMVC.Controllers
{
    public class ClienteController : Controller
    {
        private readonly IClienteService _clienteService;
        private readonly IServicoRepository _servicoRepository;

        public ClienteController(
            IClienteService clienteService,
            IServicoRepository servicoRepository
        )
        {
            _clienteService = clienteService;
            _servicoRepository = servicoRepository;
        }

        public IActionResult MenuPrincipal()
        {
            return View();
        }

        public async Task<IActionResult> SolicitarServico()
        {
            var servicos = await _servicoRepository.GetAllAsync();
            return View("SolicitarServico", servicos);
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
        public async Task<IActionResult> Create(ClienteDTO clienteDto)
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
                ClienteId = cliente.ClienteId,
                Nome = cliente.Nome,
                Email = cliente.Email,
                Telefone = cliente.Telefone,
            };

            return View(clienteDto);
        }

        // Atualiza um cliente existente
        [HttpPost]
        public async Task<IActionResult> Edit(int id, ClienteDTO clienteDto)
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
