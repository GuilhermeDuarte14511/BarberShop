using BarberShop.Application.Services;
using BarberShop.Domain.Entities;
using BarberShop.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace BarberShopMVC.Controllers
{
    public class BarbeariaController : BaseController
    {
        private readonly IBarbeariaRepository _barbeariaRepository;

        public BarbeariaController(ILogService logService, IBarbeariaRepository barbeariaRepository) : base(logService)
        {
            _barbeariaRepository = barbeariaRepository;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // Recupera o ID da barbearia da sessão
                var barbeariaId = HttpContext.Session.GetInt32("BarbeariaId");
                if (barbeariaId == null)
                {
                    await LogAsync("WARNING", nameof(BarbeariaController), "Tentativa de acesso sem barbearia na sessão", "ID da sessão nulo");
                    return RedirectToAction("Index", "Admin"); // Redireciona se o ID não estiver na sessão
                }

                // Busca as informações da barbearia
                var barbearia = await _barbeariaRepository.GetByIdAsync((int)barbeariaId);
                if (barbearia == null)
                {
                    await LogAsync("WARNING", nameof(BarbeariaController), "Barbearia não encontrada", $"BarbeariaId: {barbeariaId}");
                    return NotFound("Barbearia não encontrada.");
                }

                // Converte a logo para Base64, caso exista
                if (barbearia.Logo != null)
                {
                    ViewData["BarbeariaLogo"] = "data:image/png;base64," + Convert.ToBase64String(barbearia.Logo);
                }

                // Log de acesso à página "Meus Dados"
                await LogAsync("INFO", nameof(BarbeariaController), "Acesso à página Meus Dados", $"BarbeariaId: {barbeariaId}");

                return View(barbearia);
            }
            catch (Exception ex)
            {
                // Log de erro com a mensagem da exceção
                await LogAsync("ERROR", nameof(BarbeariaController), "Erro ao acessar Meus Dados", ex.Message);
                return RedirectToAction("Erro", "Home"); // Redireciona para uma página de erro
            }
        }

        [HttpPost]
        public async Task<IActionResult> UploadLogo(IFormFile Logo)
        {
            try
            {
                var barbeariaId = HttpContext.Session.GetInt32("BarbeariaId");
                if (barbeariaId == null) return Json(new { success = false });

                using (var ms = new MemoryStream())
                {
                    await Logo.CopyToAsync(ms);
                    var logoBytes = ms.ToArray();

                    // Atualiza apenas a logo chamando o novo método do repositório
                    await _barbeariaRepository.AtualizarLogoAsync((int)barbeariaId, logoBytes);
                }

                // Retorna a imagem em base64 para exibição imediata
                var logoBase64 = Convert.ToBase64String(await _barbeariaRepository.ObterLogoAsync((int)barbeariaId));
                return Json(new { success = true, newLogoBase64 = logoBase64 });
            }
            catch
            {
                return Json(new { success = false });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SalvarDados(Barbearia model)
        {
            try
            {
                var barbeariaId = HttpContext.Session.GetInt32("BarbeariaId");
                if (barbeariaId == null)
                {
                    await LogAsync("WARNING", nameof(BarbeariaController), "Tentativa de salvar sem barbearia na sessão", "ID da sessão nulo");
                    return RedirectToAction("Index", "Admin");
                }

                var barbearia = await _barbeariaRepository.GetByIdAsync((int)barbeariaId);
                if (barbearia == null)
                {
                    await LogAsync("WARNING", nameof(BarbeariaController), "Barbearia não encontrada", $"BarbeariaId: {barbeariaId}");
                    return NotFound("Barbearia não encontrada.");
                }

                // Concatenando Endereco e Numero para salvar no banco
                barbearia.Endereco = $"{model.Endereco}, {model.Numero}".Trim();

                // Atualizar demais propriedades que podem ter sido alteradas
                barbearia.Telefone = model.Telefone;
                barbearia.Email = model.Email;
                barbearia.CEP = model.CEP;
                barbearia.Cidade = model.Cidade;
                barbearia.Estado = model.Estado;
                barbearia.HorarioFuncionamento = model.HorarioFuncionamento;
                barbearia.Status = model.Status;

                await _barbeariaRepository.UpdateAsync(barbearia);

                await LogAsync("INFO", nameof(BarbeariaController), "Dados da barbearia atualizados com sucesso", $"BarbeariaId: {barbeariaId}");

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                await LogAsync("ERROR", nameof(BarbeariaController), "Erro ao salvar dados da barbearia", ex.Message);
                return RedirectToAction("Erro", "Home");
            }
        }



    }
}
