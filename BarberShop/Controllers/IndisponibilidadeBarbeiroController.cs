using BarberShop.Application.Interfaces;
using BarberShop.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using BarberShop.Application.Services;

namespace BarberShopMVC.Controllers
{
    public class IndisponibilidadeBarbeiroController : BaseController
    {
        private readonly IIndisponibilidadeService _indisponibilidadeService;
        private readonly IOnboardingService _onboardingService; // Serviço de onboarding

        public IndisponibilidadeBarbeiroController(
            IIndisponibilidadeService indisponibilidadeService,
            IOnboardingService onboardingService,
            ILogService logService
        ) : base(logService)
        {
            _indisponibilidadeService = indisponibilidadeService;
            _onboardingService = onboardingService;
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

                int usuarioId = ObterUsuarioIdLogado(); // Método para obter o ID do usuário logado

                // Verifica o status do onboarding
                bool onboardingConcluido = await _onboardingService.VerificarProgressoAsync(usuarioId, "Indisponibilidades");
                ViewData["ShowOnboarding"] = onboardingConcluido ? "false" : "true";

                // Obtém as indisponibilidades da barbearia
                var indisponibilidades = await _indisponibilidadeService.ObterIndisponibilidadesPorBarbeariaAsync(barbeariaId.Value);

                // Log de acesso
                await LogAsync(
                    "INFO",
                    nameof(IndisponibilidadeBarbeiroController),
                    "Acesso à página de Indisponibilidades",
                    $"BarbeariaId: {barbeariaId.Value}, UsuarioId: {usuarioId}"
                );

                return View(indisponibilidades);
            }
            catch (Exception ex)
            {
                // Log de erro
                await LogAsync("ERROR", nameof(IndisponibilidadeBarbeiroController), ex.Message, ex.ToString());
                return StatusCode(500, "Erro ao carregar a lista de indisponibilidades.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(IndisponibilidadeBarbeiro indisponibilidade)
        {
            try
            {
                if (indisponibilidade.DataInicio >= indisponibilidade.DataFim)
                {
                    return BadRequest(new { success = false, message = "A data de início deve ser anterior à data de fim." });
                }

                await _indisponibilidadeService.AdicionarIndisponibilidadeAsync(indisponibilidade);
                return Json(new { success = true, message = "Indisponibilidade adicionada com sucesso." });
            }
            catch (Exception ex)
            {
                await LogAsync("Error", "IndisponibilidadeBarbeiroController.Create", ex.Message, ex.ToString());
                return StatusCode(500, "Erro ao adicionar a indisponibilidade.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, IndisponibilidadeBarbeiro indisponibilidade)
        {
            if (id != indisponibilidade.IndisponibilidadeId)
                return BadRequest(new { success = false, message = "O ID da indisponibilidade não corresponde." });

            try
            {
                if (indisponibilidade.DataInicio >= indisponibilidade.DataFim)
                {
                    return BadRequest(new { success = false, message = "A data de início deve ser anterior à data de fim." });
                }

                await _indisponibilidadeService.AtualizarIndisponibilidadeAsync(indisponibilidade);
                return Json(new { success = true, message = "Indisponibilidade atualizada com sucesso." });
            }
            catch (Exception ex)
            {
                await LogAsync("Error", "IndisponibilidadeBarbeiroController.Edit", ex.Message, ex.ToString(), id.ToString());
                return StatusCode(500, "Erro ao atualizar a indisponibilidade.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var indisponibilidade = await _indisponibilidadeService.ObterPorIdAsync(id);
                if (indisponibilidade == null)
                {
                    return Json(new { success = false, message = "Indisponibilidade não encontrada." });
                }

                await _indisponibilidadeService.ExcluirIndisponibilidadeAsync(id);
                return Json(new { success = true, message = "Indisponibilidade excluída com sucesso." });
            }
            catch (Exception ex)
            {
                await LogAsync("Error", "IndisponibilidadeBarbeiroController.DeleteConfirmed", ex.Message, ex.ToString(), id.ToString());
                return StatusCode(500, "Erro ao excluir a indisponibilidade.");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var indisponibilidade = await _indisponibilidadeService.ObterPorIdAsync(id);
                if (indisponibilidade == null)
                {
                    return Json(new { success = false, message = "Indisponibilidade não encontrada." });
                }

                return Json(new
                {
                    indisponibilidade.IndisponibilidadeId,
                    indisponibilidade.BarbeiroId,
                    indisponibilidade.DataInicio,
                    indisponibilidade.DataFim,
                    indisponibilidade.Motivo
                });
            }
            catch (Exception ex)
            {
                await LogAsync("Error", "IndisponibilidadeBarbeiroController.Details", ex.Message, ex.ToString(), id.ToString());
                return StatusCode(500, "Erro ao buscar detalhes da indisponibilidade.");
            }
        }
    }
}
