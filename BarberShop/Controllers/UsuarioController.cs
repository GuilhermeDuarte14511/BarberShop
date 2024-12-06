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
        private readonly IAgendamentoService _agendamentoService;
        private readonly IIndisponibilidadeService _indisponibilidadeService;
        private readonly IOnboardingService _onboardingService;

        public UsuarioController(
            IUsuarioService usuarioService,
            IBarbeiroService barbeiroService,
            IBarbeiroServicoService barbeiroServicoService,
            IEmailService emailService,
            IAutenticacaoService autenticacaoService,
            IIndisponibilidadeService indisponibilidadeService,
            IAgendamentoService agendamentoService,
            IOnboardingService onboardingService,
            ILogService logService)
            : base(logService)
        {
            _usuarioService = usuarioService;
            _barbeiroService = barbeiroService;
            _barbeiroServicoService = barbeiroServicoService;
            _emailService = emailService;
            _autenticacaoService = autenticacaoService;
            _indisponibilidadeService = indisponibilidadeService;
            _agendamentoService = agendamentoService;
            _onboardingService = onboardingService;
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
                // Obtém o ID da barbearia da sessão
                int barbeariaId = HttpContext.Session.GetInt32("BarbeariaId").Value;

                // Verifica se já existe usuário com o mesmo e-mail ou telefone
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

                // Cria a entidade do usuário
                var usuario = new Usuario
                {
                    Nome = request.Nome,
                    Email = request.Email,
                    Telefone = request.Telefone,
                    Role = request.Role,
                    Status = request.Status.Value,
                    BarbeariaId = barbeariaId
                };

                // Gera a senha aleatória e realiza o hash
                var senhaAleatoria = GerarSenhaAleatoria();
                var senhaAleatoriaDescriptografada = senhaAleatoria;
                senhaAleatoria = _autenticacaoService.HashPassword(senhaAleatoria);
                usuario.SenhaHash = senhaAleatoria;

                // Se for barbeiro, cria o registro na tabela de barbeiros
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

                // Salva o usuário no banco de dados
                var usuarioCriado = await _usuarioService.CriarUsuarioAsync(usuario);

                // Define as telas do onboarding de acordo com o tipo de usuário
                var telasOnboarding = new List<string>();
                if (request.TipoUsuario == "Admin")
                {
                    telasOnboarding = new List<string>
                    {
                        "Dashboard", "Barbeiros", "Servicos", "Pagamentos", "Agendamentos",
                        "Meus Dados", "Horarios", "Feriados", "Indisponibilidade", "Usuarios", "Avaliacoes"
                    };
                        }
                        else if (request.TipoUsuario == "Barbeiro")
                        {
                            telasOnboarding = new List<string>
                    {
                        "Dashboard", "Meus Agendamentos", "Meus Servicos", "Minhas Datas",
                        "Meus Dados", "Minhas Avaliacoes"
                    };
                }

                // Registra os passos iniciais do onboarding
                await _onboardingService.RegistrarPassosIniciaisAsync(usuarioCriado.UsuarioId, telasOnboarding);

                // Envia o e-mail de boas-vindas com as credenciais
                var nomeBarbearia = User.FindFirst("BarbeariaNome")?.Value;
                var urlSlug = User.FindFirst("urlSlug")?.Value;

                await _emailService.EnviarEmailBoasVindasAsync(
                    usuario.Email,
                    usuario.Nome,
                    senhaAleatoriaDescriptografada,
                    request.TipoUsuario,
                    nomeBarbearia,
                    urlSlug
                );

                // Log de sucesso
                await LogAsync("Information", nameof(UsuarioController), "Usuário criado com sucesso", $"UsuarioId: {usuarioCriado.UsuarioId}");

                return Json(new { success = true, message = "Usuário criado com sucesso e credenciais enviadas por e-mail!", data = usuarioCriado });
            }
            catch (Exception ex)
            {
                // Log de erro
                await LogAsync("Error", nameof(UsuarioController), "Erro ao criar usuário", ex.Message);
                return Json(new { success = false, message = "Erro ao criar usuário." });
            }
        }



        [HttpDelete]
        public async Task<IActionResult> Deletar(int id)
        {
            try
            { 

                var baseUrl = $"{Request.Scheme}://{Request.Host.Value}";
                // Obter o usuário pelo ID
                var usuario = await _usuarioService.ObterUsuarioPorIdAsync(id);
                if (usuario == null)
                {
                    return Json(new { success = false, message = "Usuário não encontrado." });
                }

                if (usuario.BarbeiroId.HasValue)
                {
                    var barbeiroId = usuario.BarbeiroId.Value;

                    // Obter todos os agendamentos futuros associados ao barbeiro
                    var agendamentosFuturos = await _agendamentoService.ObterAgendamentosFuturosPorBarbeiroIdAsync(barbeiroId);

                    // Notificar os clientes sobre o cancelamento de seus agendamentos
                    foreach (var agendamento in agendamentosFuturos)
                    {
                        await _emailService.EnviarEmailCancelamentoAgendamentoAsync(
                            agendamento.Cliente.Email,
                            agendamento.Cliente.Nome,
                            agendamento.Barbearia.Nome,
                            agendamento.DataHora,
                            usuario.Nome,
                            $"{baseUrl}/{agendamento.Barbearia.UrlSlug}" // Passa a URL base com o slug da barbearia
                        );

                        // Cancelar o agendamento
                        agendamento.Status = StatusAgendamento.Cancelado;
                        await _agendamentoService.AtualizarAgendamentoAsync(agendamento.AgendamentoId, agendamento);
                    }

                    // Obter todos os serviços vinculados ao barbeiro
                    var servicosVinculados = await _barbeiroServicoService.ObterServicosPorBarbeiroIdAsync(barbeiroId);
                    if (servicosVinculados != null && servicosVinculados.Any())
                    {
                        // Desvincular todos os serviços associados ao barbeiro
                        foreach (var servico in servicosVinculados)
                        {
                            await _barbeiroServicoService.DesvincularServicoAsync(barbeiroId, servico.ServicoId);
                        }
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

        [HttpGet]
        public IActionResult ObterClaims()
        {
            var claims = ObterTodosClaims();
            return Json(new
            {
                UsuarioId = claims["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"],
                BarbeiroId = claims["BarbeiroId"]
            });
        }

        [HttpPost]
        public async Task<IActionResult> CompletarOnboarding(string tela)
        {
            try
            {
                // Obter o ID do usuário logado a partir dos claims
                var usuarioId = ObterUsuarioIdLogado();

                // Marcar o onboarding como completo para a tela específica
                var sucesso = await _usuarioService.MarcarOnboardingComoCompletoAsync(usuarioId, tela);

                if (!sucesso)
                    return BadRequest(new { message = "Erro ao atualizar o status do onboarding para a tela." });

                return Ok(new { message = "Onboarding concluído com sucesso!" });
            }
            catch (Exception ex)
            {
                await LogAsync("Error", nameof(UsuarioController), "Erro ao completar onboarding", ex.Message);
                return StatusCode(500, new { message = "Erro interno ao completar o onboarding." });
            }
        }


        [HttpGet]
        public async Task<IActionResult> VerificarProgresso(string tela)
        {
            try
            {
                // Obtém o ID do usuário logado
                var usuarioId = ObterUsuarioIdLogado();

                // Verifica o progresso para a tela especificada
                var progresso = await _onboardingService.VerificarProgressoAsync(usuarioId, tela);

                return Json(new { concluido = progresso });
            }
            catch (Exception ex)
            {
                await LogAsync("Error", nameof(UsuarioController), "Erro ao verificar progresso do onboarding", ex.Message);
                return StatusCode(500, new { message = "Erro ao verificar o progresso do onboarding." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SalvarProgresso(string tela)
        {
            try
            {
                // Obtém o ID do usuário logado
                var usuarioId = ObterUsuarioIdLogado();

                // Salva o progresso para a tela especificada
                await _onboardingService.SalvarProgressoAsync(usuarioId, tela);

                return Ok(new { message = "Progresso salvo com sucesso!" });
            }
            catch (Exception ex)
            {
                await LogAsync("Error", nameof(UsuarioController), "Erro ao salvar progresso do onboarding", ex.Message);
                return StatusCode(500, new { message = "Erro ao salvar o progresso do onboarding." });
            }
        }

    }
}
