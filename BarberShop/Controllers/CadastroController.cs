using BarberShop.Application.DTOs;
using BarberShop.Application.Interfaces;
using BarberShop.Application.Services;
using BarberShop.Domain.Entities;
using BarberShop.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CadastroController : ControllerBase
    {
        private readonly IBarbeariaRepository _barbeariaRepository;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IAutenticacaoService _autenticacaoService;

        public CadastroController(
            IBarbeariaRepository barbeariaRepository,
            IUsuarioRepository usuarioRepository,
            IAutenticacaoService autenticacaoService)
        {
            _barbeariaRepository = barbeariaRepository;
            _usuarioRepository = usuarioRepository;
            _autenticacaoService = autenticacaoService;
        }

        [HttpPost("CadastrarBarbeariaAdmin")]
        public async Task<IActionResult> CadastrarBarbeariaAdmin([FromBody] CadastroRequestDTO model)
        {
            // Valida se todos os dados necessários foram enviados
            if (string.IsNullOrEmpty(model.NomeBarbearia) || string.IsNullOrEmpty(model.Cidade) ||
                string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.NomeAdmin) || string.IsNullOrEmpty(model.Senha))
            {
                return BadRequest("Todos os campos são obrigatórios.");
            }

            // Gera o UrlSlug inicial e garante sua unicidade
            var urlSlug = GerarUrlSlug(model.NomeBarbearia);
            int slugSuffix = 1;
            string uniqueUrlSlug = urlSlug;

            // Verifica se já existe uma barbearia com o mesmo UrlSlug no banco de dados
            while (await _barbeariaRepository.ExistsByUrlSlugAsync(uniqueUrlSlug))
            {
                uniqueUrlSlug = $"{urlSlug}-{slugSuffix}";
                slugSuffix++;
            }

            try
            {
                // Cria e salva a barbearia com o UrlSlug único
                var barbearia = new Barbearia
                {
                    Nome = model.NomeBarbearia,
                    Endereco = model.Endereco,
                    Cidade = model.Cidade,
                    Estado = model.Estado,
                    Telefone = model.TelefoneBarbearia,
                    CEP = model.Cep,
                    Email = model.Email,
                    UrlSlug = uniqueUrlSlug
                };

                // Adiciona e salva a barbearia
                await _barbeariaRepository.AddAsync(barbearia);
                await _barbeariaRepository.SaveChangesAsync();

                // Cria e salva o administrador associado à barbearia
                var admin = new Usuario
                {
                    Nome = model.NomeAdmin,
                    Email = model.Email,
                    Telefone = model.TelefoneAdmin,
                    SenhaHash = _autenticacaoService.HashPassword(model.Senha),
                    Role = "Admin",
                    BarbeariaId = barbearia.BarbeariaId
                };

                await _usuarioRepository.AddAsync(admin);
                await _usuarioRepository.SaveChangesAsync();

                // Retorna o resultado incluindo o usuário e a barbearia
                return Ok(new
                {
                    success = true,
                    urlSlug = barbearia.UrlSlug,
                    message = "Cadastro realizado com sucesso.",
                    usuarioId = admin.UsuarioId,
                    barbeariaId = barbearia.BarbeariaId
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro ao salvar a barbearia e o usuário:", ex);
                return StatusCode(500, "Erro ao salvar a barbearia e o usuário.");
            }
        }


        [NonAction]
        // Função para gerar o UrlSlug a partir do nome da barbearia
        public string GerarUrlSlug(string nomeBarbearia)
        {
            var urlSlug = nomeBarbearia
                .ToLower() // Converte tudo para minúsculas
                .Replace(" ", "")  // Remove espaços
                .Normalize(NormalizationForm.FormD)  // Remove acentos
                .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark) // Remove caracteres não espaçantes
                .ToArray();

            return new string(urlSlug);
        }

        // Novo Método: Excluir Barbearia e Usuário
        [HttpPost("ExcluirBarbeariaEUsuario")]
        public async Task<IActionResult> ExcluirBarbeariaEUsuario(int barbeariaId, int usuarioId)
        {
            try
            {
                // Excluindo o usuário
                var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);
                if (usuario == null) return NotFound("Usuário não encontrado.");

                await _usuarioRepository.DeleteAsync(usuario);

                // Excluindo a barbearia
                var barbearia = await _barbeariaRepository.GetByIdAsync(barbeariaId);
                if (barbearia == null) return NotFound("Barbearia não encontrada.");

                await _barbeariaRepository.DeleteAsync(barbearia);

                return Ok(new { success = true, message = "Barbearia e usuário excluídos com sucesso." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}
