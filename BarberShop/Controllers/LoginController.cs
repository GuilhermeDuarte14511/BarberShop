using BarberShop.Application.DTOs;
using BarberShop.Application.Interfaces;
using BarberShop.Application.Services;
using BarberShop.Domain.Entities;
using BarberShop.Domain.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BarberShopMVC.Controllers
{
    public class LoginController : BaseController
    {
        private readonly IClienteRepository _clienteRepository;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IEmailService _emailService;
        private readonly IAutenticacaoService _autenticacaoService;
        private readonly IBarbeariaRepository _barbeariaRepository;
        private readonly IRedefinirSenhaService _redefinirSenhaService;


        public LoginController(
            IClienteRepository clienteRepository,
            IUsuarioRepository usuarioRepository,
            IEmailService emailService,
            IAutenticacaoService autenticacaoService,
            IBarbeariaRepository barbeariaRepository,
            IRedefinirSenhaService redefinirSenhaService,
            ILogService logService) : base(logService)
        {
            _clienteRepository = clienteRepository;
            _usuarioRepository = usuarioRepository;
            _emailService = emailService;
            _autenticacaoService = autenticacaoService;
            _barbeariaRepository = barbeariaRepository;
            _redefinirSenhaService = redefinirSenhaService;
        }

        public async Task<IActionResult> Login(string barbeariaUrl)
        {
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    var barbeariaUrlSession = HttpContext.Session.GetString("BarbeariaUrl");
                    if (!string.IsNullOrEmpty(barbeariaUrlSession) && barbeariaUrlSession == barbeariaUrl)
                    {
                        return RedirectToAction("MenuPrincipal", "Cliente", new { barbeariaUrl });
                    }
                }

                var barbearia = await _barbeariaRepository.GetByUrlSlugAsync(barbeariaUrl);

                if (barbearia != null)
                {
                    HttpContext.Session.SetInt32("BarbeariaId", barbearia.BarbeariaId);
                    HttpContext.Session.SetString("BarbeariaUrl", barbeariaUrl);

                    ViewData["BarbeariaNome"] = barbearia.Nome;
                    if (barbearia.Logo != null)
                    {
                        ViewData["BarbeariaLogo"] = "data:image/png;base64," + Convert.ToBase64String(barbearia.Logo);
                    }

                    return View();
                }
                else
                {
                    await LogAsync("Warning", "Login", "Barbearia não encontrada", $"Url: {barbeariaUrl}");
                    return RedirectToAction("BarbeariaNaoEncontrada", "Erro");
                }
            }
            catch (Exception ex)
            {
                await LogAsync("Error", "Login", $"Erro ao carregar login: {ex.Message}", $"Url: {barbeariaUrl}");
                return RedirectToAction("Error", "Erro");
            }
        }

        [HttpGet]
        public async Task<IActionResult> AdminLogin(string barbeariaUrl)
        {
            try
            {
                if (User.Identity.IsAuthenticated && User.IsInRole("Admin") || User.IsInRole("Admin"))
                {
                    var barbeariaUrlSession = HttpContext.Session.GetString("BarbeariaUrl");
                    if (!string.IsNullOrEmpty(barbeariaUrlSession) && barbeariaUrlSession == barbeariaUrl)
                    {
                        return RedirectToAction("Index", "Admin", new { barbeariaUrl });
                    }
                }

                var barbearia = await _barbeariaRepository.GetByUrlSlugAsync(barbeariaUrl);

                if (barbearia != null)
                {
                    HttpContext.Session.SetInt32("BarbeariaId", barbearia.BarbeariaId);
                    HttpContext.Session.SetString("BarbeariaUrl", barbeariaUrl);

                    ViewData["BarbeariaNome"] = barbearia.Nome;
                    ViewData["BarbeariaUrl"] = barbeariaUrl;
                    if (barbearia.Logo != null)
                    {
                        ViewData["BarbeariaLogo"] = "data:image/png;base64," + Convert.ToBase64String(barbearia.Logo);
                    }

                    return View("AdminLogin");
                }
                else
                {
                    await LogAsync("Warning", "AdminLogin", "Barbearia não encontrada", $"Url: {barbeariaUrl}");
                    return RedirectToAction("BarbeariaNaoEncontrada", "Erro");
                }
            }
            catch (Exception ex)
            {
                await LogAsync("Error", "AdminLogin", $"Erro ao carregar login administrativo: {ex.Message}", $"Url: {barbeariaUrl}");
                return RedirectToAction("Error", "Erro");
            }
        }

        [HttpPost]
        public async Task<IActionResult> AdminLogin(string email, string password)
        {
            try
            {
                var usuario = await _usuarioRepository.GetByEmailAsync(email);
                if (usuario == null ||!_autenticacaoService.VerifyPassword(password, usuario.SenhaHash) || (usuario.Role != "Admin" && usuario.Role != "Barbeiro"))
                {
                    return Json(new { success = false, message = "Credenciais inválidas ou usuário não é administrador." });
                }

                var barbeariaId = HttpContext.Session.GetInt32("BarbeariaId");
                if (!barbeariaId.HasValue)
                {
                    return Json(new { success = false, message = "Erro ao identificar a barbearia." });
                }

                var barbearia = await _barbeariaRepository.GetByIdAsync(barbeariaId.Value);
                string codigoVerificacao = GerarCodigoVerificacao();
                usuario.CodigoValidacao = codigoVerificacao;
                usuario.CodigoValidacaoExpiracao = DateTime.UtcNow.AddMinutes(5);
                await _usuarioRepository.UpdateCodigoVerificacaoAsync(usuario.UsuarioId, codigoVerificacao, usuario.CodigoValidacaoExpiracao);
                await _emailService.EnviarEmailCodigoVerificacaoAsync(usuario.Email, usuario.Nome, codigoVerificacao, barbearia?.Nome);

                await LogAsync("Info", "AdminLogin", $"Código de verificação enviado para o administrador {usuario.UsuarioId}", $"Email: {email}");
                return Json(new { success = true, usuarioId = usuario.UsuarioId });
            }
            catch (Exception ex)
            {
                await LogAsync("Error", "AdminLogin", $"Erro ao realizar login de administrador: {ex.Message}", $"Email: {email}");
                return Json(new { success = false, message = "Erro interno ao realizar login." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> VerificarAdminCodigo(int usuarioId, string codigo)
        {
            try
            {
                // Obter o usuário pelo ID
                var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);

                // Validar usuário e código de validação
                if (usuario == null || usuario.CodigoValidacaoExpiracao < DateTime.UtcNow || codigo != usuario.CodigoValidacao)
                {
                    return Json(new { success = false, message = "Código inválido ou expirado." });
                }

                if (usuario.BarbeariaId <= 0)
                {
                    return Json(new { success = false, message = "Usuário não está vinculado a uma barbearia válida." });
                }

                // Buscar a barbearia pelo ID
                var barbearia = await _barbeariaRepository.GetByIdAsync(usuario.BarbeariaId);
                if (barbearia == null)
                {
                    return Json(new { success = false, message = "Barbearia não encontrada." });
                }

                // Criar os claims para autenticação
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, usuario.UsuarioId.ToString()),
                    new Claim(ClaimTypes.Name, usuario.Nome),
                    new Claim(ClaimTypes.Role, usuario.Role),
                    new Claim("BarbeariaId", usuario.BarbeariaId.ToString()),
                    new Claim("BarbeariaNome", barbearia.Nome),
                    new Claim("urlSlug", barbearia.UrlSlug)
                };

                // Adicionar o claim de BarbeiroId, se aplicável
                if (usuario.BarbeiroId.HasValue)
                {
                    claims.Add(new Claim("BarbeiroId", usuario.BarbeiroId.Value.ToString()));
                }

                // Criar identidade e principal
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                // Configurar propriedades de autenticação
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                };

                // Realizar o login
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal, authProperties);

                // Recuperar a URL da barbearia da sessão
                var barbeariaUrl = HttpContext.Session.GetString("BarbeariaUrl");

                // Registrar log de sucesso
                await LogAsync("Info", nameof(VerificarAdminCodigo), $"Administrador {usuarioId} autenticado com sucesso.", $"UsuarioId: {usuarioId}");
                return Json(new { success = true, redirectUrl = Url.Action("Index", "Admin", new { barbeariaUrl }) });
            }
            catch (Exception ex)
            {
                // Registrar log de erro
                await LogAsync("Error", nameof(VerificarAdminCodigo), $"Erro ao verificar código de administrador: {ex.Message}", $"UsuarioId: {usuarioId}");
                return Json(new { success = false, message = "Erro ao verificar código." });
            }
        }


        [HttpPost]
        public async Task<IActionResult> Login(string inputFieldLogin, string passwordInputLogin)
        {
            try
            {
                if (string.IsNullOrEmpty(inputFieldLogin) || string.IsNullOrEmpty(passwordInputLogin))
                {
                    return Json(new { success = false, message = "Por favor, insira um telefone, email e senha válidos." });
                }

                bool isEmail = inputFieldLogin.Contains("@");
                string emailInput = isEmail ? inputFieldLogin : null;
                string phoneInput = isEmail ? null : inputFieldLogin;

                int? barbeariaId = HttpContext.Session.GetInt32("BarbeariaId");
                string barbeariaUrl = HttpContext.Session.GetString("BarbeariaUrl");

                if (!barbeariaId.HasValue || string.IsNullOrEmpty(barbeariaUrl))
                {
                    return Json(new { success = false, message = "Erro ao identificar a barbearia." });
                }

                var cliente = await _clienteRepository.GetByEmailOrPhoneAsync(emailInput, phoneInput, barbeariaId.Value);

                if (cliente != null)
                {
                    if (_autenticacaoService.VerifyPassword(passwordInputLogin, cliente.Senha))
                    {
                        var claimsPrincipal = _autenticacaoService.AutenticarCliente(cliente);
                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

                        HttpContext.Session.SetString("BarbeariaUrl", barbeariaUrl);
                        ViewData["BarbeariaUrl"] = barbeariaUrl;

                        return Json(new { success = true, redirectUrl = Url.Action("MenuPrincipal", "Cliente", new { barbeariaUrl }) });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Senha incorreta. Tente novamente." });
                    }
                }
                else
                {
                    return Json(new { success = false, message = "Cliente não encontrado. Revise a informação e tente novamente." });
                }
            }
            catch (Exception ex)
            {
                await LogAsync("Error", "Login", $"Erro ao realizar login: {ex.Message}", $"Input: {inputFieldLogin}");
                return Json(new { success = false, message = "Erro interno ao realizar login." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Cadastro(string nameInput, string registerEmailInput, string registerPhoneInput, string passwordInput)
        {
            try
            {
                if (string.IsNullOrEmpty(registerEmailInput) || string.IsNullOrEmpty(registerPhoneInput) || string.IsNullOrEmpty(nameInput) || string.IsNullOrEmpty(passwordInput))
                {
                    return Json(new { success = false, message = "Todos os campos são obrigatórios." });
                }

                int? barbeariaId = HttpContext.Session.GetInt32("BarbeariaId");
                if (!barbeariaId.HasValue)
                {
                    return Json(new { success = false, message = "Erro ao identificar a barbearia." });
                }

                var clienteExistente = await _clienteRepository.GetByEmailOrPhoneAsync(registerEmailInput, registerPhoneInput, barbeariaId.Value);
                if (clienteExistente != null)
                {
                    return Json(new { success = false, message = "Este email ou telefone já está cadastrado." });
                }

                var cliente = new Cliente
                {
                    Nome = nameInput,
                    Email = registerEmailInput,
                    Telefone = registerPhoneInput,
                    Senha = _autenticacaoService.HashPassword(passwordInput),
                    Role = "Cliente",
                    BarbeariaId = barbeariaId.Value
                };

                await _clienteRepository.AddAsync(cliente);
                await _clienteRepository.SaveChangesAsync();

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, cliente.ClienteId.ToString()),
                    new Claim(ClaimTypes.Name, cliente.Nome),
                    new Claim(ClaimTypes.Role, cliente.Role)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                var authProperties = new AuthenticationProperties { IsPersistent = true };
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal, authProperties);

                var redirectUrl = Url.Action("MenuPrincipal", "Cliente");

                await LogAsync("Info", "Cadastro", "Cliente cadastrado e autenticado com sucesso.", $"ClienteId: {cliente.ClienteId}");
                return Json(new { success = true, redirectUrl });
            }
            catch (Exception ex)
            {
                await LogAsync("Error", "Cadastro", $"Erro ao realizar cadastro: {ex.Message}", null);
                return Json(new { success = false, message = "Erro interno ao realizar cadastro." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> VerificarCodigo(int clienteId, string codigo)
        {
            try
            {
                var cliente = await _clienteRepository.GetByIdAsync(clienteId);

                if (cliente == null || cliente.CodigoValidacaoExpiracao < DateTime.UtcNow || codigo != cliente.CodigoValidacao)
                {
                    return Json(new { success = false, message = "Código inválido ou expirado." });
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, cliente.ClienteId.ToString()),
                    new Claim(ClaimTypes.Name, cliente.Nome),
                    new Claim(ClaimTypes.Role, cliente.Role)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                var authProperties = new AuthenticationProperties { IsPersistent = true };
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal, authProperties);

                var redirectUrl = cliente.Role == "Admin" ? Url.Action("Index", "Admin") : Url.Action("MenuPrincipal", "Cliente");

                await _clienteRepository.UpdateAsync(cliente);

                await LogAsync("Info", "VerificarCodigo", "Código verificado e cliente autenticado com sucesso.", $"ClienteId: {clienteId}");
                return Json(new { success = true, redirectUrl });
            }
            catch (Exception ex)
            {
                await LogAsync("Error", "VerificarCodigo", $"Erro ao verificar código: {ex.Message}", $"ClienteId: {clienteId}");
                return Json(new { success = false, message = "Erro ao verificar código." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ReenviarCodigo(int clienteId)
        {
            try
            {
                var cliente = await _clienteRepository.GetByIdAsync(clienteId);

                if (cliente == null)
                {
                    return Json(new { success = false, message = "Cliente não encontrado." });
                }

                int? barbeariaId = HttpContext.Session.GetInt32("BarbeariaId");
                if (!barbeariaId.HasValue)
                {
                    return Json(new { success = false, message = "Erro ao identificar a barbearia." });
                }

                var barbearia = await _barbeariaRepository.GetByIdAsync(barbeariaId.Value);

                string codigoVerificacao = GerarCodigoVerificacao();
                cliente.CodigoValidacao = codigoVerificacao;
                cliente.CodigoValidacaoExpiracao = DateTime.UtcNow.AddMinutes(5);
                await _clienteRepository.UpdateAsync(cliente);

                await _emailService.EnviarEmailCodigoVerificacaoAsync(cliente.Email, cliente.Nome, codigoVerificacao, barbearia?.Nome);

                await LogAsync("Info", "ReenviarCodigo", "Código de verificação reenviado com sucesso.", $"ClienteId: {clienteId}");
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                await LogAsync("Error", "ReenviarCodigo", $"Erro ao reenviar código de verificação: {ex.Message}", $"ClienteId: {clienteId}");
                return Json(new { success = false, message = "Erro ao reenviar código de verificação." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ReenviarCodigoAdm(int usuarioId)
        {
            try
            {
                var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);

                if (usuario == null || usuario.Role != "Admin")
                {
                    return Json(new { success = false, message = "Usuário administrador não encontrado." });
                }

                var barbeariaId = HttpContext.Session.GetInt32("BarbeariaId");
                if (!barbeariaId.HasValue)
                {
                    return Json(new { success = false, message = "Erro ao identificar a barbearia." });
                }

                var barbearia = await _barbeariaRepository.GetByIdAsync(barbeariaId.Value);

                string codigoVerificacao = GerarCodigoVerificacao();
                usuario.CodigoValidacao = codigoVerificacao;
                usuario.CodigoValidacaoExpiracao = DateTime.UtcNow.AddMinutes(5);
                await _usuarioRepository.UpdateCodigoVerificacaoAsync(usuario.UsuarioId, codigoVerificacao, usuario.CodigoValidacaoExpiracao);
                await _emailService.EnviarEmailCodigoVerificacaoAsync(usuario.Email, usuario.Nome, codigoVerificacao, barbearia?.Nome);

                await LogAsync("Info", "ReenviarCodigoAdm", "Código de verificação para admin reenviado com sucesso.", $"UsuarioId: {usuarioId}");
                return Json(new { success = true, message = "Novo código de verificação enviado para o email." });
            }
            catch (Exception ex)
            {
                await LogAsync("Error", "ReenviarCodigoAdm", $"Erro ao reenviar código de verificação para admin: {ex.Message}", $"UsuarioId: {usuarioId}");
                return Json(new { success = false, message = "Erro ao reenviar código de verificação." });
            }
        }
            
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            try
            {
                // Faz o logout do usuário
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                // Log do evento de logout
                await LogAsync("Info", "Logout", "Usuário deslogado com sucesso.", null);

                // Recupera a URL da barbearia armazenada na sessão
                var barbeariaUrl = HttpContext.Session.GetString("BarbeariaUrl");

                // Verifica se existe a URL e redireciona para a página inicial da barbearia
                if (!string.IsNullOrEmpty(barbeariaUrl))
                {
                    return RedirectToAction("Login", "Login", new { barbeariaUrl });
                }

                // Caso não exista a URL, redireciona para uma página padrão
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                // Log de erro
                await LogAsync("Error", "Logout", $"Erro ao deslogar usuário: {ex.Message}", null);

                // Redireciona para página de erro
                return RedirectToAction("Error", "Erro");
            }
        }

        [HttpPost]
        public async Task<IActionResult> LogoutAdmin()
        {
            try
            {
                // Faz o logout do administrador
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                // Log do evento de logout administrativo
                await LogAsync("Info", "LogoutAdmin", "Administrador deslogado com sucesso.", null);

                // Recupera a URL da barbearia armazenada na sessão
                var barbeariaUrl = HttpContext.Session.GetString("BarbeariaUrl");

                // Verifica se existe a URL e redireciona para o formato simplificado
                if (!string.IsNullOrEmpty(barbeariaUrl))
                {
                    return Redirect($"/{barbeariaUrl}/admin");
                }

                // Caso não exista a URL, redireciona para uma página padrão
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                // Log de erro
                await LogAsync("Error", "LogoutAdmin", $"Erro ao deslogar administrador: {ex.Message}", null);

                // Redireciona para página de erro
                return RedirectToAction("Error", "Erro");
            }
        }


        [HttpPost]
        public async Task<IActionResult> SolicitarRecuperacaoSenha([FromBody] string email)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                {
                    return Json(new { success = false, message = "E-mail não fornecido." });
                }

                var cliente = await _clienteRepository.GetByEmailAsync(email);
                if (cliente == null)
                {
                    return Json(new { success = false, message = "E-mail não encontrado." });
                }

                var token = Guid.NewGuid().ToString();
                cliente.TokenRecuperacaoSenha = token;
                cliente.TokenExpiracao = DateTime.UtcNow.AddHours(1);
                await _clienteRepository.UpdateAsync(cliente);

                var linkRecuperacao = Url.Action("RedefinirSenha", "Login", new { clienteId = cliente.ClienteId, token }, Request.Scheme);
                await _emailService.EnviarEmailRecuperacaoSenhaAsync(cliente.Email, cliente.Nome, linkRecuperacao);

                await LogAsync("Info", "SolicitarRecuperacaoSenha", "E-mail de recuperação de senha enviado com sucesso.", $"ClienteId: {cliente.ClienteId}");
                return Json(new { success = true, message = "Instruções de recuperação de senha enviadas para o e-mail." });
            }
            catch (Exception ex)
            {
                await LogAsync("Error", "SolicitarRecuperacaoSenha", $"Erro ao solicitar recuperação de senha: {ex.Message}", $"Email: {email}");
                return Json(new { success = false, message = "Erro ao processar a solicitação de recuperação de senha." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> RedefinirSenha(int clienteId, string token)
        {
            if (!await _redefinirSenhaService.ValidarTokenAsync(clienteId, token))
            {
                var urlRedirecionamento = await _redefinirSenhaService.ObterUrlRedirecionamentoAsync(clienteId);
                ViewData["RedirecionamentoUrl"] = urlRedirecionamento;
                return View("TokenInvalido");
            }

            ViewData["ClienteId"] = clienteId;
            ViewData["Token"] = token;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RedefinirSenha([FromBody] RedefinirSenhaDto redefinirSenhaDto)
        {
            if (!await _redefinirSenhaService.RedefinirSenhaAsync(redefinirSenhaDto))
            {
                return Json(new { success = false, message = "Token inválido ou expirado." });
            }

            var urlRedirecionamento = await _redefinirSenhaService.ObterUrlRedirecionamentoAsync(redefinirSenhaDto.ClienteId);
            return Json(new { success = true, message = "Senha redefinida com sucesso.", redirectUrl = urlRedirecionamento });
        }


        [HttpPost]
        public async Task<IActionResult> SolicitarRecuperacaoSenhaAdmin([FromBody] string email)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                {
                    return Json(new { success = false, message = "E-mail não fornecido." });
                }

                var usuario = await _usuarioRepository.GetByEmailAsync(email);
                if (usuario == null || usuario.Role != "Admin")
                {
                    return Json(new { success = false, message = "E-mail não encontrado ou usuário não é administrador." });
                }

                var token = Guid.NewGuid().ToString();
                usuario.TokenRecuperacaoSenha = token;
                usuario.TokenExpiracao = DateTime.UtcNow.AddHours(1);
                await _usuarioRepository.UpdateAsync(usuario);

                var linkRecuperacao = Url.Action("RedefinirSenhaAdmin", "Login", new { usuarioId = usuario.UsuarioId, token }, Request.Scheme);
                await _emailService.EnviarEmailRecuperacaoSenhaAsync(usuario.Email, usuario.Nome, linkRecuperacao);

                await LogAsync("Info", "SolicitarRecuperacaoSenhaAdmin", "E-mail de recuperação de senha enviado com sucesso.", $"UsuarioId: {usuario.UsuarioId}");
                return Json(new { success = true, message = "Instruções de recuperação de senha enviadas para o e-mail." });
            }
            catch (Exception ex)
            {
                await LogAsync("Error", "SolicitarRecuperacaoSenhaAdmin", $"Erro ao solicitar recuperação de senha: {ex.Message}", $"Email: {email}");
                return Json(new { success = false, message = "Erro ao processar a solicitação de recuperação de senha." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> RedefinirSenhaAdmin(int usuarioId, string token)
        {
            try
            {
                var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);

                if (usuario == null || usuario.TokenRecuperacaoSenha != token || usuario.TokenExpiracao < DateTime.UtcNow)
                {
                    await LogAsync("Warning", "RedefinirSenhaAdmin", "Token de redefinição de senha inválido ou expirado.", $"UsuarioId: {usuarioId}");

                    var barbearia = await _barbeariaRepository.GetByIdAsync(usuario.BarbeariaId);
                    if (barbearia == null)
                    {
                        await LogAsync("Error", "RedefinirSenhaAdmin", "Barbearia associada não encontrada.", $"UsuarioId: {usuarioId}");
                        return RedirectToAction("Error", "Erro");
                    }

                    var urlRedirecionamento = Url.Action("AdminLogin", "Login", new { area = "", barbeariaUrl = barbearia.UrlSlug }, Request.Scheme);

                    ViewData["RedirecionamentoUrl"] = urlRedirecionamento;
                    return View("TokenInvalido");
                }

                ViewData["UsuarioId"] = usuario.UsuarioId;
                ViewData["Token"] = token;
                return View();
            }
            catch (Exception ex)
            {
                await LogAsync("Error", "RedefinirSenhaAdmin", $"Erro ao carregar a página de redefinição de senha: {ex.Message}", $"UsuarioId: {usuarioId}");
                return RedirectToAction("Error", "Erro");
            }
        }


        [HttpPost]
        public async Task<IActionResult> RedefinirSenhaAdmin([FromBody] RedefinirSenhaDto redefinirSenhaDto)
        {
            try
            {
                var usuario = await _usuarioRepository.GetByIdAsync(redefinirSenhaDto.ClienteId);
                if (usuario == null || usuario.TokenRecuperacaoSenha != redefinirSenhaDto.Token || usuario.TokenExpiracao < DateTime.UtcNow)
                {
                    await LogAsync("Warning", "RedefinirSenhaAdmin", "Token de redefinição de senha inválido ou expirado.", $"UsuarioId: {redefinirSenhaDto.ClienteId}");
                    return Json(new { success = false, message = "Token inválido ou expirado." });
                }

                usuario.SenhaHash = _autenticacaoService.HashPassword(redefinirSenhaDto.NovaSenha);
                usuario.TokenRecuperacaoSenha = null;
                usuario.TokenExpiracao = null;
                await _usuarioRepository.UpdateAsync(usuario);

                var barbearia = await _barbeariaRepository.GetByIdAsync(usuario.BarbeariaId);
                if (barbearia == null)
                {
                    await LogAsync("Error", "RedefinirSenhaAdmin", "Barbearia não encontrada ao redefinir senha.", $"UsuarioId: {redefinirSenhaDto.ClienteId}");
                    return Json(new { success = false, message = "Barbearia não encontrada." });
                }

                var urlRedirecionamento = Url.Action("AdminLogin", "Login", new { area = "", barbeariaUrl = barbearia.UrlSlug }, Request.Scheme);

                await LogAsync("Info", "RedefinirSenhaAdmin", "Senha redefinida com sucesso.", $"UsuarioId: {redefinirSenhaDto.ClienteId}");
                return Json(new { success = true, message = "Senha redefinida com sucesso.", redirectUrl = urlRedirecionamento });
            }
            catch (Exception ex)
            {
                await LogAsync("Error", "RedefinirSenhaAdmin", $"Erro ao redefinir senha: {ex.Message}", $"UsuarioId: {redefinirSenhaDto.ClienteId}");
                return Json(new { success = false, message = "Erro ao redefinir senha." });
            }
        }



        private string GerarCodigoVerificacao()
        {
            Random random = new Random();
            return random.Next(100000, 999999).ToString();
        }
    }
}
