using BarberShop.Application.Services;
using BarberShop.Domain.Entities;
using BarberShop.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace BarberShopMVC.Controllers
{
    public class ServicoController : BaseController
    {
        private readonly IServicoRepository _servicoRepository;

        public ServicoController(IServicoRepository servicoRepository, ILogService logService)
            : base(logService)
        {
            _servicoRepository = servicoRepository;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var servicos = await _servicoRepository.GetAllAsync();
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
                var servico = await _servicoRepository.GetByIdAsync(id);
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
        public async Task<IActionResult> Create(Servico servico)
        {
            try
            {
                await _servicoRepository.AddAsync(servico);
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
        public async Task<IActionResult> Edit(int id, Servico servico)
        {
            try
            {
                if (id != servico.ServicoId || !ModelState.IsValid)
                {
                    await LogAsync("WARNING", nameof(ServicoController), "Dados inválidos para edição do serviço", $"Id: {id}");
                    return Json(new { success = false, message = "Dados inválidos." });
                }

                await _servicoRepository.UpdateAsync(servico);
                await LogAsync("INFO", nameof(ServicoController), "Serviço atualizado com sucesso", $"Id: {id}");
                return Json(new { success = true, message = "Serviço atualizado com sucesso." });
            }
            catch (Exception ex)
            {
                await LogAsync("ERROR", nameof(ServicoController), "Erro ao atualizar serviço", ex.Message);
                return Json(new { success = false, message = "Erro ao atualizar serviço.", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var servico = await _servicoRepository.GetByIdAsync(id);
                if (servico == null)
                {
                    await LogAsync("WARNING", nameof(ServicoController), "Serviço não encontrado para exclusão", $"Id: {id}");
                    return NotFound();
                }

                await _servicoRepository.DeleteAsync(id);
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
                var servicos = await _servicoRepository.GetAllAsync();
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
