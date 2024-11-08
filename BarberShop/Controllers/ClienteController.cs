using BarberShop.Application.DTOs;
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

        public IActionResult MenuPrincipal(string barbeariaUrl)
        {
            // Verifica se o valor está na sessão, caso não tenha sido passado como parâmetro
            if (string.IsNullOrEmpty(barbeariaUrl))
            {
                barbeariaUrl = HttpContext.Session.GetString("BarbeariaUrl") ?? "NomeBarbearia";
            }
            else
            {
                // Salva a URL na sessão para acessos futuros
                HttpContext.Session.SetString("BarbeariaUrl", barbeariaUrl);
            }

            ViewData["BarbeariaUrl"] = barbeariaUrl;
            return View();
        }



        public async Task<IActionResult> Index(string barbeariaUrl)
        {
            ViewData["BarbeariaUrl"] = barbeariaUrl;
            int barbeariaId = int.Parse(HttpContext.Session.GetString("BarbeariaId") ?? "0");
            var clientes = await _clienteService.ObterTodosClientesAsync(barbeariaId);
            return View(clientes);
        }

        public async Task<IActionResult> Details(int id, string barbeariaUrl)
        {
            ViewData["BarbeariaUrl"] = barbeariaUrl;
            int barbeariaId = int.Parse(HttpContext.Session.GetString("BarbeariaId") ?? "0");
            var cliente = await _clienteService.ObterClientePorIdAsync(id, barbeariaId);
            if (cliente == null) return NotFound();
            return View(cliente);
        }

        public IActionResult Create(string barbeariaUrl)
        {
            ViewData["BarbeariaUrl"] = barbeariaUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(ClienteDTO clienteDto, string barbeariaUrl)
        {
            ViewData["BarbeariaUrl"] = barbeariaUrl;
            if (ModelState.IsValid)
            {
                var cliente = new Cliente
                {
                    Nome = clienteDto.Nome,
                    Email = clienteDto.Email,
                    Telefone = clienteDto.Telefone
                };
                int barbeariaId = int.Parse(HttpContext.Session.GetString("BarbeariaId") ?? "0");
                await _clienteService.AdicionarClienteAsync(cliente, barbeariaId);
                return RedirectToAction(nameof(Index), new { barbeariaUrl });
            }
            return View(clienteDto);
        }

        public async Task<IActionResult> Edit(int id, string barbeariaUrl)
        {
            ViewData["BarbeariaUrl"] = barbeariaUrl;
            int barbeariaId = int.Parse(HttpContext.Session.GetString("BarbeariaId") ?? "0");
            var cliente = await _clienteService.ObterClientePorIdAsync(id, barbeariaId);
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

        [HttpPost]
        public async Task<IActionResult> Edit(int id, ClienteDTO clienteDto, string barbeariaUrl)
        {
            ViewData["BarbeariaUrl"] = barbeariaUrl;
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
                int barbeariaId = int.Parse(HttpContext.Session.GetString("BarbeariaId") ?? "0");
                await _clienteService.AtualizarClienteAsync(cliente, barbeariaId);
                return RedirectToAction(nameof(Index), new { barbeariaUrl });
            }
            return View(clienteDto);
        }

        public async Task<IActionResult> Delete(int id, string barbeariaUrl)
        {
            ViewData["BarbeariaUrl"] = barbeariaUrl;
            int barbeariaId = int.Parse(HttpContext.Session.GetString("BarbeariaId") ?? "0");
            var cliente = await _clienteService.ObterClientePorIdAsync(id, barbeariaId);
            if (cliente == null) return NotFound();
            return View(cliente);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id, string barbeariaUrl)
        {
            int barbeariaId = int.Parse(HttpContext.Session.GetString("BarbeariaId") ?? "0");
            await _clienteService.DeletarClienteAsync(id, barbeariaId);
            return RedirectToAction(nameof(Index), new { barbeariaUrl });
        }

        public async Task<IActionResult> SolicitarServico(string barbeariaUrl)
        {
            ViewData["BarbeariaUrl"] = barbeariaUrl;
            int? barbeariaId = HttpContext.Session.GetInt32("BarbeariaId");

            if (barbeariaId == null)
            {
                return RedirectToAction("BarbeariaNaoEncontrada", "Erro");
            }

            ViewData["BarbeariaId"] = barbeariaId; // Passa o barbeariaId para a ViewData
            var servicos = await _servicoRepository.ObterServicosPorBarbeariaIdAsync(barbeariaId.Value);
            return View("SolicitarServico", servicos);
        }
    }
}
