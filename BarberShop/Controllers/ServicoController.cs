using BarberShop.Application.Interfaces;
using BarberShop.Application.Services;
using BarberShop.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace BarberShopMVC.Controllers
{
    public class ServicoController : BaseController
    {
        private readonly IServicoService _servicoService;

        public ServicoController(IServicoService servicoService, ILogService logService)
            : base(logService)
        {
            _servicoService = servicoService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                int? barbeariaId = HttpContext.Session.GetInt32("BarbeariaId");
                if (!barbeariaId.HasValue)
                {
                    return RedirectToAction("BarbeariaNaoEncontrada", "Erro");
                }

                var servicos = await _servicoService.ObterTodosPorBarbeariaIdAsync(barbeariaId.Value);
                await LogAsync("INFO", nameof(ServicoController), "Carregando lista de serviços", "");
                return View(servicos);
            }
            catch (Exception ex)
            {
                await LogAsync("ERROR", nameof(ServicoController), "Erro ao carregar serviços", ex.Message);
                return Json(new { success = false, message = "Erro ao carregar serviços.", error = ex.Message });
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var servico = await _servicoService.ObterPorIdAsync(id);
                if (servico == null)
                {
                    await LogAsync("WARNING", nameof(ServicoController), "Serviço não encontrado", $"Id: {id}");
                    return NotFound();
                }

                await LogAsync("INFO", nameof(ServicoController), "Detalhes do serviço carregados", $"Id: {id}");
                return Json(servico);
            }
            catch (Exception ex)
            {
                await LogAsync("ERROR", nameof(ServicoController), "Erro ao obter detalhes do serviço", ex.Message);
                return Json(new { success = false, message = "Erro ao obter detalhes do serviço.", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Servico servico)
        {
            try
            {
                int? barbeariaId = HttpContext.Session.GetInt32("BarbeariaId");
                if (!barbeariaId.HasValue)
                {
                    return BadRequest(new { success = false, message = "Barbearia não encontrada na sessão." });
                }

                servico.BarbeariaId = barbeariaId.Value; // Associa o serviço à barbearia
                await _servicoService.AdicionarAsync(servico);
                await LogAsync("INFO", nameof(ServicoController), "Serviço adicionado com sucesso", $"Nome: {servico.Nome}");
                return Json(new { success = true, message = "Serviço adicionado com sucesso." });
            }
            catch (Exception ex)
            {
                await LogAsync("ERROR", nameof(ServicoController), "Erro ao adicionar serviço", ex.Message);
                return Json(new { success = false, message = "Erro ao adicionar serviço.", error = ex.Message });
            }
        }


        [HttpPost]
        public async Task<IActionResult> Edit(int id, [FromBody] Servico servico)
        {
            if (id != servico.ServicoId)
            {
                await LogAsync("WARNING", nameof(ServicoController), "ID do serviço não corresponde.", $"Id: {id}");
                return Json(new { success = false, message = "Dados inválidos." });
            }

            try
            {
                int? barbeariaId = HttpContext.Session.GetInt32("BarbeariaId");
                if (!barbeariaId.HasValue)
                {
                    return BadRequest(new { success = false, message = "Barbearia não encontrada na sessão." });
                }

                servico.BarbeariaId = barbeariaId;

                await _servicoService.AtualizarAsync(servico);
                return Json(new { success = true, message = "Serviço atualizado com sucesso." });
            }
            catch (Exception ex)
            {
                await LogAsync("ERROR", nameof(ServicoController), "Erro ao atualizar o serviço", ex.Message, id.ToString());
                return StatusCode(500, new { success = false, message = "Erro ao atualizar o serviço." });
            }
        }


        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                int? barbeariaId = HttpContext.Session.GetInt32("BarbeariaId");
                if (!barbeariaId.HasValue)
                {
                    return BadRequest(new { success = false, message = "Barbearia não encontrada na sessão." });
                }

                var servico = await _servicoService.ObterPorIdAsync(id);
                if (servico == null || servico.BarbeariaId != barbeariaId.Value)
                {
                    await LogAsync("WARNING", nameof(ServicoController), "Serviço não encontrado para exclusão", $"Id: {id}");
                    return NotFound();
                }

                await _servicoService.ExcluirAsync(id);
                await LogAsync("INFO", nameof(ServicoController), "Serviço excluído com sucesso", $"Id: {id}");
                return Json(new { success = true, message = "Serviço excluído com sucesso." });
            }
            catch (Exception ex)
            {
                await LogAsync("ERROR", nameof(ServicoController), "Erro ao excluir serviço", ex.Message);
                return Json(new { success = false, message = "Erro ao excluir serviço.", error = ex.Message });
            }
        }

        public async Task<IActionResult> List()
        {
            try
            {
                int? barbeariaId = HttpContext.Session.GetInt32("BarbeariaId");
                if (!barbeariaId.HasValue)
                {
                    return RedirectToAction("BarbeariaNaoEncontrada", "Erro");
                }

                var servicos = await _servicoService.ObterTodosPorBarbeariaIdAsync(barbeariaId.Value);
                return PartialView("_ServicoListPartial", servicos);
            }
            catch (Exception ex)
            {
                await LogAsync("ERROR", nameof(ServicoController), "Erro ao carregar lista de serviços", ex.Message);
                return Json(new { success = false, message = "Erro ao carregar lista de serviços.", error = ex.Message });
            }
        }
    }
}
