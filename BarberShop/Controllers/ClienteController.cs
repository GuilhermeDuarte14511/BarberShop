using BarberShop.Application.DTOs;
using BarberShop.Application.Services;
using BarberShop.Domain.Entities;
using BarberShop.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BarberShopMVC.Controllers
{
    public class ClienteController : BaseController
    {
        private readonly IClienteService _clienteService;
        private readonly IServicoRepository _servicoRepository;
        private readonly IAutenticacaoService _autenticacaoService;

        public ClienteController(
            IClienteService clienteService,
            IServicoRepository servicoRepository,
            IAutenticacaoService autenticacaoService,
            ILogService logService
        ) : base(logService)
        {
            _clienteService = clienteService;
            _servicoRepository = servicoRepository;
            _autenticacaoService = autenticacaoService;
        }

        public IActionResult MenuPrincipal(string barbeariaUrl)
        {
            if (string.IsNullOrEmpty(barbeariaUrl))
            {
                barbeariaUrl = HttpContext.Session.GetString("BarbeariaUrl") ?? "NomeBarbearia";
            }
            else
            {
                HttpContext.Session.SetString("BarbeariaUrl", barbeariaUrl);
            }

            ViewData["BarbeariaUrl"] = barbeariaUrl;
            return View();
        }

        public async Task<IActionResult> Index(string barbeariaUrl)
        {
            await LogAsync("INFO", nameof(Index), "Iniciando listagem de clientes.", $"BarbeariaUrl: {barbeariaUrl}");
            try
            {
                int barbeariaId = int.Parse(HttpContext.Session.GetString("BarbeariaId") ?? "0");
                var clientes = await _clienteService.ObterTodosClientesAsync(barbeariaId);
                return View(clientes);
            }
            catch (Exception ex)
            {
                await LogAsync("ERROR", nameof(Index), $"Erro ao listar clientes: {ex.Message}", $"BarbeariaUrl: {barbeariaUrl}");
                throw;
            }
        }

        public async Task<IActionResult> Details(int id, string barbeariaUrl)
        {
            await LogAsync("INFO", nameof(Details), "Solicitação de detalhes do cliente.", $"ClienteId: {id}, BarbeariaUrl: {barbeariaUrl}");
            try
            {
                int barbeariaId = int.Parse(HttpContext.Session.GetString("BarbeariaId") ?? "0");
                var cliente = await _clienteService.ObterClientePorIdAsync(id, barbeariaId);
                if (cliente == null)
                {
                    await LogAsync("WARNING", nameof(Details), "Cliente não encontrado.", $"ClienteId: {id}");
                    return NotFound();
                }
                return View(cliente);
            }
            catch (Exception ex)
            {
                await LogAsync("ERROR", nameof(Details), $"Erro ao buscar cliente: {ex.Message}", $"ClienteId: {id}");
                throw;
            }
        }

        public IActionResult Create(string barbeariaUrl)
        {
            ViewData["BarbeariaUrl"] = barbeariaUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(ClienteDTO clienteDto, string barbeariaUrl)
        {
            await LogAsync("INFO", nameof(Create), "Solicitação de criação de cliente recebida.", $"BarbeariaUrl: {barbeariaUrl}");
            if (ModelState.IsValid)
            {
                try
                {
                    var cliente = new Cliente
                    {
                        Nome = clienteDto.Nome,
                        Email = clienteDto.Email,
                        Telefone = clienteDto.Telefone
                    };
                    int barbeariaId = int.Parse(HttpContext.Session.GetString("BarbeariaId") ?? "0");
                    await _clienteService.AdicionarClienteAsync(cliente, barbeariaId);
                    await LogAsync("INFO", nameof(Create), "Cliente criado com sucesso.", $"ClienteId: {cliente.ClienteId}");
                    return RedirectToAction(nameof(Index), new { barbeariaUrl });
                }
                catch (Exception ex)
                {
                    await LogAsync("ERROR", nameof(Create), $"Erro ao criar cliente: {ex.Message}", $"DadosCliente: {clienteDto}");
                    throw;
                }
            }
            return View(clienteDto);
        }

        public async Task<IActionResult> Edit(int id, string barbeariaUrl)
        {
            await LogAsync("INFO", nameof(Edit), "Solicitação de edição de cliente recebida.", $"ClienteId: {id}, BarbeariaUrl: {barbeariaUrl}");
            try
            {
                int barbeariaId = int.Parse(HttpContext.Session.GetString("BarbeariaId") ?? "0");
                var cliente = await _clienteService.ObterClientePorIdAsync(id, barbeariaId);
                if (cliente == null)
                {
                    await LogAsync("WARNING", nameof(Edit), "Cliente não encontrado.", $"ClienteId: {id}");
                    return NotFound();
                }

                var clienteDto = new ClienteDTO
                {
                    ClienteId = cliente.ClienteId,
                    Nome = cliente.Nome,
                    Email = cliente.Email,
                    Telefone = cliente.Telefone,
                };

                return View(clienteDto);
            }
            catch (Exception ex)
            {
                await LogAsync("ERROR", nameof(Edit), $"Erro ao carregar cliente para edição: {ex.Message}", $"ClienteId: {id}");
                throw;
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, ClienteDTO clienteDto, string barbeariaUrl)
        {
            await LogAsync("INFO", nameof(Edit), "Solicitação de atualização de cliente recebida.", $"ClienteId: {id}, BarbeariaUrl: {barbeariaUrl}");
            if (id != clienteDto.ClienteId)
            {
                await LogAsync("WARNING", nameof(Edit), "ID do cliente não corresponde ao fornecido.", $"ClienteId: {id}");
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                try
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
                    await LogAsync("INFO", nameof(Edit), "Cliente atualizado com sucesso.", $"ClienteId: {cliente.ClienteId}");
                    return RedirectToAction(nameof(Index), new { barbeariaUrl });
                }
                catch (Exception ex)
                {
                    await LogAsync("ERROR", nameof(Edit), $"Erro ao atualizar cliente: {ex.Message}", $"DadosCliente: {clienteDto}");
                    throw;
                }
            }
            return View(clienteDto);
        }

        public async Task<IActionResult> Delete(int id, string barbeariaUrl)
        {
            await LogAsync("INFO", nameof(Delete), "Solicitação de exclusão de cliente recebida.", $"ClienteId: {id}, BarbeariaUrl: {barbeariaUrl}");
            try
            {
                int barbeariaId = int.Parse(HttpContext.Session.GetString("BarbeariaId") ?? "0");
                var cliente = await _clienteService.ObterClientePorIdAsync(id, barbeariaId);
                if (cliente == null)
                {
                    await LogAsync("WARNING", nameof(Delete), "Cliente não encontrado.", $"ClienteId: {id}");
                    return NotFound();
                }
                return View(cliente);
            }
            catch (Exception ex)
            {
                await LogAsync("ERROR", nameof(Delete), $"Erro ao carregar cliente para exclusão: {ex.Message}", $"ClienteId: {id}");
                throw;
            }
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id, string barbeariaUrl)
        {
            await LogAsync("INFO", nameof(DeleteConfirmed), "Solicitação de exclusão de cliente confirmada.", $"ClienteId: {id}, BarbeariaUrl: {barbeariaUrl}");
            try
            {
                int barbeariaId = int.Parse(HttpContext.Session.GetString("BarbeariaId") ?? "0");
                await _clienteService.DeletarClienteAsync(id, barbeariaId);
                await LogAsync("INFO", nameof(DeleteConfirmed), "Cliente excluído com sucesso.", $"ClienteId: {id}");
                return RedirectToAction(nameof(Index), new { barbeariaUrl });
            }
            catch (Exception ex)
            {
                await LogAsync("ERROR", nameof(DeleteConfirmed), $"Erro ao excluir cliente: {ex.Message}", $"ClienteId: {id}");
                throw;
            }
        }

        public async Task<IActionResult> SolicitarServico(string barbeariaUrl)
        {
            await LogAsync("INFO", nameof(SolicitarServico), "Listando serviços disponíveis.", $"BarbeariaUrl: {barbeariaUrl}");
            try
            {
                int? barbeariaId = HttpContext.Session.GetInt32("BarbeariaId");
                if (!barbeariaId.HasValue)
                {
                    await LogAsync("WARNING", nameof(SolicitarServico), "Barbearia não identificada na sessão.", null);
                    return RedirectToAction("BarbeariaNaoEncontrada", "Erro");
                }

                var servicos = await _servicoRepository.ObterServicosPorBarbeariaIdAsync(barbeariaId.Value);
                return View("SolicitarServico", servicos);
            }
            catch (Exception ex)
            {
                await LogAsync("ERROR", nameof(SolicitarServico), $"Erro ao listar serviços: {ex.Message}", null);
                throw;
            }
        }

        [HttpPost]
        public async Task<IActionResult> AlterarSenhaCliente([FromBody] AlterarSenhaDto alterarSenhaDto)
        {
            try
            {
                var clienteId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                int? barbeariaId = HttpContext.Session.GetInt32("BarbeariaId");

                await LogAsync("INFO", nameof(AlterarSenhaCliente), "Solicitação de alteração de senha recebida.", $"ClienteId: {clienteId}");


                if (!barbeariaId.HasValue)
                {
                    return Json(new { success = false, message = "Barbearia não encontrada na sessão." });
                }

                var cliente = await _clienteService.ObterClientePorIdAsync(clienteId, barbeariaId.Value);
                if (cliente == null || !_autenticacaoService.VerifyPassword(alterarSenhaDto.SenhaAtual, cliente.Senha))
                {
                    return Json(new { success = false, message = "Usuário ou senha inválidos." });
                }

                cliente.Senha = _autenticacaoService.HashPassword(alterarSenhaDto.NovaSenha);
                await _clienteService.AtualizarClienteAsync(cliente, barbeariaId.Value);
                await LogAsync("INFO", nameof(AlterarSenhaCliente), "Senha alterada com sucesso.", $"ClienteId: {cliente.ClienteId}");
                return Json(new { success = true, message = "Senha alterada com sucesso!" });
            }
            catch (Exception ex)
            {
                await LogAsync("ERROR", nameof(AlterarSenhaCliente), $"Erro ao alterar senha: {ex.Message}", null);
                return Json(new { success = false, message = "Erro ao alterar senha." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> MeusDadosCliente()
        {
            await LogAsync("INFO", nameof(MeusDadosCliente), "Solicitação de dados do cliente.", null);
            try
            {
                var clienteId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                int? barbeariaId = HttpContext.Session.GetInt32("BarbeariaId");

                if (!barbeariaId.HasValue)
                {
                    await LogAsync("WARNING", nameof(MeusDadosCliente), "Barbearia não identificada na sessão.", null);
                    return RedirectToAction("Erro", "Home", new { mensagem = "Barbearia não identificada na sessão." });
                }

                var cliente = await _clienteService.ObterClientePorIdAsync(clienteId, barbeariaId.Value);
                if (cliente == null)
                {
                    await LogAsync("WARNING", nameof(MeusDadosCliente), "Cliente não encontrado.", $"ClienteId: {clienteId}");
                    return NotFound("Cliente não encontrado.");
                }

                var clienteDto = new ClienteDTO
                {
                    ClienteId = cliente.ClienteId,
                    Nome = cliente.Nome,
                    Email = cliente.Email,
                    Telefone = cliente.Telefone
                };

                return View(clienteDto);
            }
            catch (Exception ex)
            {
                await LogAsync("ERROR", nameof(MeusDadosCliente), $"Erro ao carregar dados do cliente: {ex.Message}", null);
                return RedirectToAction("Erro", "Home", new { mensagem = "Erro ao carregar dados do cliente." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> MeusDadosCliente([FromBody] ClienteDTO clienteDto)
        {
            await LogAsync("INFO", nameof(MeusDadosCliente), "Solicitação de atualização dos dados do cliente.", $"ClienteId: {clienteDto.ClienteId}");
            try
            {
                var clienteId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                int? barbeariaId = HttpContext.Session.GetInt32("BarbeariaId");

                if (!barbeariaId.HasValue)
                {
                    await LogAsync("WARNING", nameof(MeusDadosCliente), "Barbearia não identificada na sessão.", null);
                    return Json(new { success = false, message = "Barbearia não identificada na sessão." });
                }

                if (clienteId != clienteDto.ClienteId)
                {
                    await LogAsync("WARNING", nameof(MeusDadosCliente), "Cliente não autorizado.", $"ClienteId: {clienteDto.ClienteId}");
                    return Json(new { success = false, message = "Cliente não autorizado." });
                }

                if (!ModelState.IsValid)
                {
                    await LogAsync("WARNING", nameof(MeusDadosCliente), "Dados inválidos na solicitação.", $"ClienteDto: {clienteDto}");
                    return Json(new { success = false, message = "Dados inválidos. Verifique e tente novamente." });
                }

                await _clienteService.AtualizarDadosClienteAsync(clienteDto.ClienteId, clienteDto.Nome, clienteDto.Email, clienteDto.Telefone, barbeariaId.Value);
                await LogAsync("INFO", nameof(MeusDadosCliente), "Dados do cliente atualizados com sucesso.", $"ClienteId: {clienteDto.ClienteId}");
                return Json(new { success = true, message = "Dados atualizados com sucesso!" });
            }
            catch (Exception ex)
            {
                await LogAsync("ERROR", nameof(MeusDadosCliente), $"Erro ao atualizar dados do cliente: {ex.Message}", null);
                return Json(new { success = false, message = "Erro ao atualizar os dados. Tente novamente." });
            }
        }
    }
}
