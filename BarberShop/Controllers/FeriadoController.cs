using BarberShop.Application.Interfaces;
using BarberShop.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using BarberShop.Application.Services;
using BarberShop.Domain.Interfaces;
using System.Linq;
using BarberShop.Application.DTOs;

namespace BarberShopMVC.Controllers
{
    public class FeriadoController : BaseController
    {
        private readonly IFeriadoBarbeariaRepository _feriadoBarbeariaRepository;
        private readonly IFeriadoBarbeariaService _feriadoBarbeariaService;

        public FeriadoController(
            IFeriadoBarbeariaRepository feriadoBarbeariaRepository,
            IFeriadoBarbeariaService feriadoBarbeariaService,
            ILogService logService)
            : base(logService)
        {
            _feriadoBarbeariaRepository = feriadoBarbeariaRepository;
            _feriadoBarbeariaService = feriadoBarbeariaService;
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

                // Feriados nacionais (fixos)
                var feriadosNacionais = await _feriadoBarbeariaService.ObterFeriadosNacionaisAsync();
                var feriadosFixos = feriadosNacionais.Select(f => new FeriadoDTO
                {
                    Descricao = f.Descricao,
                    Data = f.Data,
                    Recorrente = f.Recorrente,
                    FeriadoBarbeariaId = null, // Não aplicável para feriados fixos
                    Fixo = true // Não permite edição/exclusão
                });

                // Feriados personalizados da barbearia
                var feriadosBarbearia = await _feriadoBarbeariaService.ObterFeriadosPorBarbeariaAsync(barbeariaId.Value);
                var feriadosPersonalizados = feriadosBarbearia.Select(fb => new FeriadoDTO
                {
                    Descricao = fb.Descricao,
                    Data = fb.Data,
                    Recorrente = fb.Recorrente,
                    FeriadoBarbeariaId = fb.FeriadoBarbeariaId,
                    Fixo = false // Permite edição/exclusão
                });

                // Combinar as listas
                var feriadosCombinados = feriadosFixos.Concat(feriadosPersonalizados);
                return View(feriadosCombinados);
            }
            catch (Exception ex)
            {
                await LogAsync("Error", "FeriadoController.Index", ex.Message, ex.ToString());
                return StatusCode(500, "Erro ao carregar a lista de feriados.");
            }
        }



        [HttpPost]
        public async Task<IActionResult> Create(FeriadoBarbearia feriado)
        {
            try
            {
                int? barbeariaId = HttpContext.Session.GetInt32("BarbeariaId");
                if (!barbeariaId.HasValue)
                {
                    return BadRequest(new { success = false, message = "ID da barbearia não encontrado" });
                }

                // Associa o feriado à barbearia
                feriado.BarbeariaId = barbeariaId.Value;

                // Adiciona o feriado ao repositório
                await _feriadoBarbeariaRepository.AddAsync(feriado);

                // Salva as alterações no banco de dados
                await _feriadoBarbeariaRepository.SaveChangesAsync();

                return Json(new { success = true, message = "Feriado adicionado com sucesso." });
            }
            catch (Exception ex)
            {
                await LogAsync("Error", "FeriadoController.Create", ex.Message, ex.ToString());
                return StatusCode(500, "Erro ao adicionar o feriado.");
            }
        }


        [HttpPost]
        public async Task<IActionResult> Edit(int id, FeriadoBarbearia feriado)
        {
            if (id != feriado.FeriadoBarbeariaId)
                return BadRequest(new { success = false, message = "ID do feriado não corresponde." });

            try
            {
                await _feriadoBarbeariaRepository.UpdateAsync(feriado);
                return Json(new { success = true, message = "Feriado atualizado com sucesso." });
            }
            catch (Exception ex)
            {
                await LogAsync("Error", "FeriadoController.Edit", ex.Message, ex.ToString(), id.ToString());
                return StatusCode(500, "Erro ao atualizar o feriado.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var feriado = await _feriadoBarbeariaRepository.GetByIdAsync(id);
                if (feriado == null)
                {
                    return NotFound();
                }

                await _feriadoBarbeariaRepository.DeleteAsync(id);
                return Json(new { success = true, message = "Feriado excluído com sucesso." });
            }
            catch (Exception ex)
            {
                await LogAsync("Error", "FeriadoController.DeleteConfirmed", ex.Message, ex.ToString(), id.ToString());
                return StatusCode(500, "Erro ao excluir o feriado.");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                // Obtém o feriado pelo ID
                var feriado = await _feriadoBarbeariaRepository.GetByIdAsync(id);

                if (feriado == null)
                {
                    return Json(new { success = false, message = "Feriado não encontrado." });
                }

                // Retorna os detalhes do feriado no formato necessário
                return Json(new
                {
                    success = true,
                    feriadoBarbeariaId = feriado.FeriadoBarbeariaId,
                    descricao = feriado.Descricao,
                    data = feriado.Data.ToString("yyyy-MM-dd"), // Formata a data
                    recorrente = feriado.Recorrente
                });
            }
            catch (Exception ex)
            {
                await LogAsync("Error", "FeriadoController.Details", ex.Message, ex.ToString(), id.ToString());
                return StatusCode(500, "Erro ao buscar os detalhes do feriado.");
            }
        }

    }
}
