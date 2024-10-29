using BarberShop.Application.Interfaces;
using BarberShop.Application.Services;
using BarberShop.Domain.Entities;
using BarberShop.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BarberShopMVC.Controllers
{
    public class BarbeiroController : Controller
    {
        private readonly IBarbeiroRepository _barbeiroRepository;
        private readonly IBarbeiroService _barbeiroService;

        public BarbeiroController(IBarbeiroRepository barbeiroRepository, IBarbeiroService barbeiroService)
        {
            _barbeiroRepository = barbeiroRepository;
            _barbeiroService = barbeiroService;
        }

        public async Task<IActionResult> Index()
        {
            var barbeiros = await _barbeiroRepository.GetAllAsync();
            return View(barbeiros);
        }

        // Retorna os detalhes de um barbeiro específico em JSON
        public async Task<IActionResult> Details(int id)
        {
            var barbeiro = await _barbeiroRepository.GetByIdAsync(id);
            if (barbeiro == null)
            {
                return NotFound();
            }

            return Json(new
            {
                BarbeiroId = barbeiro.BarbeiroId,
                Nome = barbeiro.Nome,
                Email = barbeiro.Email,
                Telefone = barbeiro.Telefone
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create(Barbeiro barbeiro)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Dados inválidos." });
            }

            // Verifica se já existe um barbeiro com o mesmo email ou telefone
            var barbeiroExistente = await _barbeiroRepository.GetByEmailOrPhoneAsync(barbeiro.Email, barbeiro.Telefone);

            if (barbeiroExistente != null)
            {
                string mensagemErro = "Já existe um cadastro com ";
                if (barbeiroExistente.Email == barbeiro.Email)
                {
                    mensagemErro += "esse e-mail";
                }
                if (barbeiroExistente.Telefone == barbeiro.Telefone)
                {
                    mensagemErro += mensagemErro.Contains("e-mail") ? " e telefone." : "esse telefone.";
                }

                return Json(new { success = false, message = mensagemErro });
            }

            // Adiciona o novo barbeiro, pois não houve duplicação
            await _barbeiroRepository.AddAsync(barbeiro);
            return Json(new { success = true, message = "Barbeiro adicionado com sucesso." });
        }

        // Método para editar barbeiro
        [HttpPost]
        public async Task<IActionResult> Edit(int id, Barbeiro barbeiro)
        {
            if (id != barbeiro.BarbeiroId)
                return BadRequest(new { success = false, message = "ID do barbeiro não corresponde." });

            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Dados inválidos." });
            }

            await _barbeiroRepository.UpdateAsync(barbeiro);
            return Json(new { success = true, message = "Barbeiro atualizado com sucesso." });
        }

        // Método para deletar barbeiro
        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var barbeiro = await _barbeiroRepository.GetByIdAsync(id);
            if (barbeiro == null)
            {
                return NotFound();
            }

            await _barbeiroRepository.DeleteAsync(id);
            return Json(new { success = true, message = "Barbeiro excluído com sucesso." });
        }

        // Certifique-se de que o método está retornando a view corretamente
        public async Task<IActionResult> EscolherBarbeiro(int duracaoTotal, string servicoIds)
        {
            if (duracaoTotal <= 0)
            {
                return BadRequest("A duração dos serviços é inválida.");
            }

            var barbeiros = await _barbeiroService.ObterTodosBarbeirosAsync();
            ViewData["DuracaoTotal"] = duracaoTotal;
            ViewData["ServicoIds"] = servicoIds;
            return View("EscolherBarbeiro", barbeiros); // Certifique-se de que o nome da view está correto
        }


    }
}
