using BarberShop.Application.Interfaces;
using BarberShop.Application.Services;
using BarberShop.Domain.Entities;
using BarberShop.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace BarberShopMVC.Controllers
{
    public class BarbeiroController : BaseController
    {
        private readonly IBarbeiroRepository _barbeiroRepository;
        private readonly IBarbeiroService _barbeiroService;
        private readonly IBarbeiroServicoService _barbeiroServicoService;
        private readonly IIndisponibilidadeService _indisponibilidadeService;

        public BarbeiroController(
            IBarbeiroRepository barbeiroRepository,
            IBarbeiroService barbeiroService,
            IBarbeiroServicoService barbeiroServicoService,
            IIndisponibilidadeService indisponibilidadeService,
            ILogService logService)
            : base(logService)
        {
            _barbeiroRepository = barbeiroRepository;
            _barbeiroService = barbeiroService;
            _barbeiroServicoService = barbeiroServicoService;
            _indisponibilidadeService = indisponibilidadeService;
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

                var barbeiros = await _barbeiroService.ObterBarbeirosPorBarbeariaIdAsync(barbeariaId.Value);
                return View(barbeiros);
            }
            catch (Exception ex)
            {
                await LogAsync("Error", "BarbeiroController.Index", ex.Message, ex.ToString());
                return StatusCode(500, "Erro ao carregar a lista de barbeiros.");
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var barbeiro = await _barbeiroRepository.GetByIdAsync(id);
                if (barbeiro == null)
                {
                    return NotFound();
                }

                var servicos = await _barbeiroRepository.ObterServicosPorBarbeiroIdAsync(id);

                return Json(new
                {
                    BarbeiroId = barbeiro.BarbeiroId,
                    Nome = barbeiro.Nome,
                    Email = barbeiro.Email,
                    Telefone = barbeiro.Telefone,
                    Foto = barbeiro.Foto,
                    Servicos = servicos.Select(s => new
                    {
                        s.ServicoId,
                        s.Nome,
                        s.Preco,
                        s.Duracao
                    })
                });
            }
            catch (Exception ex)
            {
                await LogAsync("Error", "BarbeiroController.Details", ex.Message, ex.ToString(), id.ToString());
                return StatusCode(500, "Erro ao carregar os detalhes do barbeiro.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(Barbeiro barbeiro, IFormFile Foto)
        {
            try
            {
                int? barbeariaId = HttpContext.Session.GetInt32("BarbeariaId");
                if (!barbeariaId.HasValue)
                {
                    return BadRequest(new { success = false, message = "ID da barbearia não encontrado" });
                }

                // Associa o barbeiro à barbearia
                barbeiro.BarbeariaId = barbeariaId.Value;

                if (Foto != null && Foto.Length > 0)
                {
                    using (var ms = new MemoryStream())
                    {
                        await Foto.CopyToAsync(ms);
                        barbeiro.Foto = ms.ToArray();
                    }
                }

                var barbeiroExistente = await _barbeiroRepository.GetByEmailOrPhoneAsync(barbeiro.Email, barbeiro.Telefone);
                if (barbeiroExistente != null)
                {
                    string mensagemErro = "Já existe um cadastro com ";
                    if (barbeiroExistente.Email == barbeiro.Email)
                        mensagemErro += "esse e-mail";
                    if (barbeiroExistente.Telefone == barbeiro.Telefone)
                        mensagemErro += mensagemErro.Contains("e-mail") ? " e telefone" : " esse telefone";

                    return Json(new { success = false, message = mensagemErro });
                }

                await _barbeiroRepository.AddAsync(barbeiro);

                return Json(new { success = true, message = "Barbeiro adicionado com sucesso" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Erro ao adicionar o barbeiro" });
            }
        }


        [HttpPost]
        public async Task<IActionResult> Edit(int id, Barbeiro barbeiro)
        {
            if (id != barbeiro.BarbeiroId)
                return BadRequest(new { success = false, message = "ID do barbeiro não corresponde." });

            try
            {
                await _barbeiroRepository.UpdateAsync(barbeiro);
                return Json(new { success = true, message = "Barbeiro atualizado com sucesso." });
            }
            catch (Exception ex)
            {
                await LogAsync("Error", "BarbeiroController.Edit", ex.Message, ex.ToString(), id.ToString());
                return StatusCode(500, "Erro ao atualizar o barbeiro.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var barbeiro = await _barbeiroRepository.GetByIdAsync(id);
                if (barbeiro == null)
                {
                    return NotFound();
                }

                // Desvincular todos os serviços associados ao barbeiro
                var servicosVinculados = await _barbeiroServicoService.ObterServicosPorBarbeiroIdAsync(id);
                foreach (var servico in servicosVinculados)
                {
                    await _barbeiroServicoService.DesvincularServicoAsync(id, servico.ServicoId);
                }

                // Após desvincular, excluir o barbeiro
                await _barbeiroService.DeletarBarbeiroAsync(id);
                return Json(new { success = true, message = "Barbeiro excluído com sucesso." });
            }
            catch (Exception ex)
            {
                await LogAsync("Error", "BarbeiroController.DeleteConfirmed", ex.Message, ex.ToString(), id.ToString());
                return StatusCode(500, "Erro ao excluir o barbeiro.");
            }
        }

        public async Task<IActionResult> EscolherBarbeiro(int duracaoTotal, string servicoIds, string barbeariaUrl)
        {
            if (duracaoTotal <= 0)
            {
                return BadRequest("A duração dos serviços é inválida.");
            }

            try
            {
                int? barbeariaId = HttpContext.Session.GetInt32("BarbeariaId");
                if (!barbeariaId.HasValue)
                {
                    return RedirectToAction("BarbeariaNaoEncontrada", "Erro");
                }

                // Converter os IDs dos serviços de string para lista de inteiros
                var servicoIdList = servicoIds.Split(',').Select(int.Parse).ToList();

                // Obter os barbeiros e verificar os serviços que eles realizam
                var barbeiros = await _barbeiroService.ObterBarbeirosPorServicosAsync(barbeariaId.Value, servicoIdList);

                // Passar os dados para a View
                ViewData["DuracaoTotal"] = duracaoTotal;
                ViewData["ServicoIds"] = servicoIds;
                ViewData["BarbeariaUrl"] = barbeariaUrl;
                ViewData["BarbeariaId"] = barbeariaId;

                return View("EscolherBarbeiro", barbeiros);
            }
            catch (Exception ex)
            {
                await LogAsync("Error", "BarbeiroController.EscolherBarbeiro", ex.Message, ex.ToString());
                return StatusCode(500, "Erro ao carregar os barbeiros.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> UploadFoto(int id, IFormFile foto)
        {
            try
            {
                if (foto == null || foto.Length == 0)
                {
                    return Json(new { success = false, message = "Nenhuma foto foi selecionada." });
                }

                var barbeiro = await _barbeiroRepository.GetByIdAsync(id);
                if (barbeiro == null)
                {
                    return NotFound("Barbeiro não encontrado.");
                }

                using (var ms = new MemoryStream())
                {
                    await foto.CopyToAsync(ms);
                    barbeiro.Foto = ms.ToArray();
                }

                await _barbeiroRepository.UpdateAsync(barbeiro);
                var fotoBase64 = Convert.ToBase64String(barbeiro.Foto);
                return Json(new { success = true, newFotoBase64 = fotoBase64 });
            }
            catch (Exception ex)
            {
                await LogAsync("Error", "BarbeiroController.UploadFoto", ex.Message, ex.ToString());
                return StatusCode(500, "Erro ao fazer o upload da foto.");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ObterBarbeirosPorBarbearia()
        {
            try
            {
                int? barbeariaId = HttpContext.Session.GetInt32("BarbeariaId");
                if (!barbeariaId.HasValue)
                {
                    return Json(new { success = false, message = "ID da barbearia não encontrado na sessão." });
                }

                var barbeiros = await _barbeiroService.ObterBarbeirosPorBarbeariaIdAsync(barbeariaId.Value);

                if (!barbeiros.Any())
                {
                    return Json(new { success = true, message = "Nenhum barbeiro encontrado para esta barbearia.", barbeiros = new List<object>() });
                }

                var barbeirosFormatados = barbeiros.Select(b => new
                {
                    BarbeiroId = b.BarbeiroId,
                    Nome = b.Nome
                });

                return Json(new { success = true, barbeiros = barbeirosFormatados });
            }
            catch (Exception ex)
            {
                await LogAsync("Error", "BarbeiroController.ObterBarbeirosPorBarbearia", ex.Message, ex.ToString());
                return StatusCode(500, "Erro ao carregar os barbeiros.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> DesvincularServico(int barbeiroId, int servicoId)
        {
            try
            {
                var sucesso = await _barbeiroServicoService.DesvincularServicoAsync(barbeiroId, servicoId);
                if (!sucesso)
                {
                    return Json(new { success = false, message = "Serviço não encontrado ou já desvinculado." });
                }

                return Json(new { success = true, message = "Serviço desvinculado com sucesso." });
            }
            catch (Exception ex)
            {
                await LogAsync("Error", "BarbeiroController.DesvincularServico", ex.Message, ex.ToString());
                return StatusCode(500, "Erro ao desvincular o serviço.");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ObterServicosNaoVinculados(int barbeiroId)
        {
            try
            {
                if (barbeiroId <= 0)
                {
                    return BadRequest(new { message = "ID do barbeiro inválido." });
                }

                var servicos = await _barbeiroServicoService.ObterServicosNaoVinculadosAsync(barbeiroId);

                return Json(servicos.Select(s => new
                {
                    s.ServicoId,
                    s.Nome,
                    s.Preco,
                    s.Duracao
                }));
            }
            catch (Exception ex)
            {
                await LogAsync("Error", "BarbeiroController.ObterServicosNaoVinculados", ex.Message, ex.ToString(), barbeiroId.ToString());
                return StatusCode(500, new { message = "Erro ao obter os serviços não vinculados." });
            }
        }



        [HttpPost]
        public async Task<IActionResult> VincularServico(int barbeiroId, int servicoId)
        {
            try
            {
                await _barbeiroServicoService.VincularServicoAsync(barbeiroId, servicoId);

                var servico = await _barbeiroServicoService.ObterServicoPorIdAsync(servicoId);

                // Retorne apenas os dados necessários
                return Json(new
                {
                    success = true,
                    servico = new
                    {
                        servico.ServicoId,
                        servico.Nome,
                        servico.Preco,
                        servico.Duracao
                    }
                });
            }
            catch (Exception ex)
            {
                await LogAsync("Error", "BarbeiroController.VincularServico", ex.Message, ex.ToString(), $"{barbeiroId}, {servicoId}");
                return StatusCode(500, new { message = "Erro ao vincular serviço." });
            }
        }




    }
}
