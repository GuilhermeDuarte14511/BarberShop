using BarberShop.Application.DTOs;
using BarberShop.Application.Interfaces;
using BarberShop.Application.Services;
using BarberShop.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BarberShop.Application.Controllers
{
    public class UsuarioController : BaseController
    {
        private readonly IUsuarioService _usuarioService;
        private readonly IBarbeiroService _barbeiroService;
        private readonly IBarbeiroServicoService _barbeiroServicoService;
        private readonly IEmailService _emailService;
        private readonly IAutenticacaoService _autenticacaoService;
        private readonly IIndisponibilidadeService _indisponibilidadeService;

        public UsuarioController(
            IUsuarioService usuarioService,
            IBarbeiroService barbeiroService,
            IBarbeiroServicoService barbeiroServicoService,
            IEmailService emailService,
            IAutenticacaoService autenticacaoService,
            IIndisponibilidadeService indisponibilidadeService,
            ILogService logService)
            : base(logService)
        {
            _usuarioService = usuarioService;
            _barbeiroService = barbeiroService;
            _barbeiroServicoService = barbeiroServicoService;
            _emailService = emailService;
            _autenticacaoService = autenticacaoService;
            _indisponibilidadeService = indisponibilidadeService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                int barbeariaId = HttpContext.Session.GetInt32("BarbeariaId").Value;
                var usuarios = await _usuarioService.ListarUsuariosPorBarbeariaAsync(barbeariaId);
                ViewData["BarbeariaId"] = barbeariaId;
                await LogAsync("Information", nameof(UsuarioController), "Listagem de usuários realizada", $"BarbeariaId: {barbeariaId}");
                return View(usuarios);
            }
            catch (Exception ex)
            {
                await LogAsync("Error", nameof(UsuarioController), "Erro ao listar usuários", ex.Message);
                return RedirectToAction("Error", "Home");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ObterUsuario(int id)
        {
            try
            {
                var usuario = await _usuarioService.ObterUsuarioPorIdAsync(id);
                if (usuario == null)
                {
                    return Json(new { success = false, message = "Usuário não encontrado." });
                }
                return Json(new { success = true, data = usuario });
            }
            catch (Exception ex)
            {
                await LogAsync("Error", nameof(UsuarioController), "Erro ao obter detalhes do usuário", ex.Message);
                return Json(new { success = false, message = "Erro interno ao buscar o usuário." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Criar([FromBody] CriarUsuarioDTO request)
        {
            try
            {
                int barbeariaId = HttpContext.Session.GetInt32("BarbeariaId").Value;
                var usuariosExistentes = await _usuarioService.ObterUsuariosPorEmailOuTelefoneAsync(request.Email, request.Telefone);
                if (usuariosExistentes != null && usuariosExistentes.Any())
                {
                    string mensagemErro = "Já existe um cadastro com ";

                    if (usuariosExistentes.Any(u => u.Email == request.Email))
                        mensagemErro += "esse e-mail";
                    if (usuariosExistentes.Any(u => u.Telefone == request.Telefone))
                        mensagemErro += mensagemErro.Contains("e-mail") ? " e telefone" : " esse telefone";

                    return Json(new { success = false, message = mensagemErro });
                }

                var usuario = new Usuario
                {
                    Nome = request.Nome,
                    Email = request.Email,
                    Telefone = request.Telefone,
                    Role = request.Role,
                    Status = request.Status.Value,
                    BarbeariaId = barbeariaId
                };

                var senhaAleatoria = GerarSenhaAleatoria();
                usuario.SenhaHash = senhaAleatoria;

                if (request.TipoUsuario == "Barbeiro")
                {
                    var barbeiro = new Barbeiro
                    {
                        Nome = request.Nome,
                        Email = request.Email,
                        Telefone = request.Telefone,
                        BarbeariaId = barbeariaId
                    };

                    barbeiro = await _barbeiroService.CriarBarbeiroAsync(barbeiro);
                    usuario.BarbeiroId = barbeiro.BarbeiroId;
                }

                var usuarioCriado = await _usuarioService.CriarUsuarioAsync(usuario);
                var nomeBarbearia = User.FindFirst("BarbeariaNome")?.Value;
                var urlSlug = User.FindFirst("urlSlug")?.Value;

                await _emailService.EnviarEmailBoasVindasAsync(
                    usuario.Email,
                    usuario.Nome,
                    senhaAleatoria,
                    request.TipoUsuario,
                    nomeBarbearia,
                    urlSlug
                );

                await LogAsync("Information", nameof(UsuarioController), "Usuário criado com sucesso", $"UsuarioId: {usuarioCriado.UsuarioId}");
                return Json(new { success = true, message = "Usuário criado com sucesso e credenciais enviadas por e-mail!", data = usuarioCriado });
            }
            catch (Exception ex)
            {
                await LogAsync("Error", nameof(UsuarioController), "Erro ao criar usuário", ex.Message);
                return Json(new { success = false, message = "Erro ao criar usuário." });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> Deletar(int id)
        {
            try
            {
                // Obter o usuário pelo ID
                var usuario = await _usuarioService.ObterUsuarioPorIdAsync(id);
                if (usuario == null)
                {
                    return Json(new { success = false, message = "Usuário não encontrado." });
                }

                if (usuario.BarbeiroId.HasValue)
                {
                    var barbeiroId = usuario.BarbeiroId.Value;

                    // Obter todas as indisponibilidades associadas ao barbeiro
                    var indisponibilidades = await _indisponibilidadeService.ObterIndisponibilidadesPorBarbeiroAsync(barbeiroId);

                    // Excluir todas as indisponibilidades do barbeiro
                    foreach (var indisponibilidade in indisponibilidades)
                    {
                        await _indisponibilidadeService.ExcluirIndisponibilidadeAsync(indisponibilidade.IndisponibilidadeId);
                    }

                    // Excluir o barbeiro
                    var barbeiroExcluido = await _barbeiroService.DeletarBarbeiroAsync(barbeiroId);
                    if (!barbeiroExcluido)
                    {
                        return Json(new { success = false, message = "Erro ao excluir o barbeiro vinculado ao usuário." });
                    }
                }

                // Excluir o usuário
                var sucesso = await _usuarioService.DeletarUsuarioAsync(id);
                if (!sucesso)
                {
                    return Json(new { success = false, message = "Erro ao deletar usuário." });
                }

                await LogAsync("Information", nameof(UsuarioController), "Usuário deletado com sucesso", $"UsuarioId: {id}");
                return Json(new { success = true, message = "Usuário deletado com sucesso!" });
            }
            catch (Exception ex)
            {
                await LogAsync("Error", nameof(UsuarioController), "Erro ao deletar usuário", ex.Message);
                return Json(new { success = false, message = "Erro ao deletar usuário." });
            }
        }

    }
}
