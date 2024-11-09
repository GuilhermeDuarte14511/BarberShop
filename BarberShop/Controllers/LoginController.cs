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
    public class LoginController : Controller
    {
        private readonly IClienteRepository _clienteRepository;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IEmailService _emailService;
        private readonly AutenticacaoService _autenticacaoService;
        private readonly IBarbeariaRepository _barbeariaRepository;

        public LoginController(
            IClienteRepository clienteRepository,
            IUsuarioRepository usuarioRepository,
            IEmailService emailService,
            AutenticacaoService autenticacaoService,
            IBarbeariaRepository barbeariaRepository)
        {
            _clienteRepository = clienteRepository;
            _usuarioRepository = usuarioRepository;
            _emailService = emailService;
            _autenticacaoService = autenticacaoService;
            _barbeariaRepository = barbeariaRepository;
        }

        public async Task<IActionResult> Login(string barbeariaUrl)
        {
            var barbearia = await _barbeariaRepository.GetByUrlSlugAsync(barbeariaUrl);

            if (barbearia != null)
            {
                HttpContext.Session.SetInt32("BarbeariaId", barbearia.BarbeariaId); // Armazena o Id da barbearia na sessão
                HttpContext.Session.SetString("BarbeariaUrl", barbeariaUrl); // Armazena a URL da barbearia na sessão

                ViewData["BarbeariaNome"] = barbearia.Nome;

                if (barbearia.Logo != null)
                {
                    ViewData["BarbeariaLogo"] = "data:image/png;base64," + Convert.ToBase64String(barbearia.Logo);
                }

                return View();
            }
            else
            {
                return RedirectToAction("BarbeariaNaoEncontrada", "Erro");
            }
        }


        [HttpGet]
        public async Task<IActionResult> AdminLogin(string barbeariaUrl)
        {
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
                return RedirectToAction("BarbeariaNaoEncontrada", "Erro");
            }
        }


        // Processa o login administrativo
        [HttpPost]
        public async Task<IActionResult> AdminLogin(string email, string password)
        {
            var usuario = await _usuarioRepository.GetByEmailAsync(email);

            if (usuario == null || !_autenticacaoService.VerifyPassword(password, usuario.SenhaHash) || usuario.Role != "Admin")
            {
                return Json(new { success = false, message = "Credenciais inválidas ou usuário não é administrador." });
            }

            string codigoVerificacao = GerarCodigoVerificacao();
            usuario.CodigoValidacao = codigoVerificacao;
            usuario.CodigoValidacaoExpiracao = DateTime.UtcNow.AddMinutes(5);
            await _usuarioRepository.UpdateCodigoVerificacaoAsync(usuario.UsuarioId, codigoVerificacao, usuario.CodigoValidacaoExpiracao);
            await _emailService.EnviarEmailCodigoVerificacaoAsync(usuario.Email, usuario.Nome, codigoVerificacao);

            return Json(new { success = true, usuarioId = usuario.UsuarioId });
        }

        [HttpPost]
        public async Task<IActionResult> VerificarAdminCodigo(int usuarioId, string codigo)
        {
            var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);

            if (usuario == null || usuario.CodigoValidacaoExpiracao < DateTime.UtcNow || codigo != usuario.CodigoValidacao)
            {
                return Json(new { success = false, message = "Código inválido ou expirado." });
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.UsuarioId.ToString()),
                new Claim(ClaimTypes.Name, usuario.Nome),
                new Claim(ClaimTypes.Role, usuario.Role)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            var authProperties = new AuthenticationProperties { IsPersistent = true };
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal, authProperties);

            // Recupera a URL da barbearia da sessão
            var barbeariaUrl = HttpContext.Session.GetString("BarbeariaUrl");

            // Redireciona para o dashboard com a URL da barbearia
            return Json(new { success = true, redirectUrl = Url.Action("Index", "Admin", new { barbeariaUrl }) });
        }


        [HttpPost]
        public async Task<IActionResult> Login(string inputFieldLogin, string passwordInputLogin)
        {
            if (string.IsNullOrEmpty(inputFieldLogin) || string.IsNullOrEmpty(passwordInputLogin))
            {
                return Json(new { success = false, message = "Por favor, insira um telefone, email e senha válidos." });
            }

            bool isEmail = inputFieldLogin.Contains("@");
            string emailInput = isEmail ? inputFieldLogin : null;
            string phoneInput = isEmail ? null : inputFieldLogin;

            // Obtém o barbeariaId e a barbeariaUrl da sessão
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

                    // Redireciona para o MenuPrincipal incluindo o barbeariaUrl
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



        [HttpPost]
        public async Task<IActionResult> Cadastro(string nameInput, string registerEmailInput, string registerPhoneInput, string passwordInput)
        {
            if (string.IsNullOrEmpty(registerEmailInput) || string.IsNullOrEmpty(registerPhoneInput) || string.IsNullOrEmpty(nameInput) || string.IsNullOrEmpty(passwordInput))
            {
                return Json(new { success = false, message = "Todos os campos são obrigatórios." });
            }

            // Obtém o barbeariaId da sessão
            int? barbeariaId = HttpContext.Session.GetInt32("BarbeariaId");
            if (!barbeariaId.HasValue)
            {
                return Json(new { success = false, message = "Erro ao identificar a barbearia." });
            }

            // Verifica se o cliente já existe para a mesma barbearia
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
                BarbeariaId = barbeariaId.Value // Atribui o barbeariaId ao cliente
            };

            await _clienteRepository.AddAsync(cliente);

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

            return Json(new { success = true, redirectUrl });
        }


        [HttpPost]
        public async Task<IActionResult> VerificarCodigo(int clienteId, string codigo)
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

            return Json(new { success = true, redirectUrl });
        }

        [HttpGet]
        public async Task<IActionResult> ReenviarCodigo(int clienteId)
        {
            var cliente = await _clienteRepository.GetByIdAsync(clienteId);

            if (cliente == null)
            {
                return Json(new { success = false, message = "Cliente não encontrado." });
            }

            string codigoVerificacao = GerarCodigoVerificacao();
            cliente.CodigoValidacao = codigoVerificacao;
            cliente.CodigoValidacaoExpiracao = DateTime.UtcNow.AddMinutes(5);
            await _clienteRepository.UpdateAsync(cliente);
            await _emailService.EnviarEmailCodigoVerificacaoAsync(cliente.Email, cliente.Nome, codigoVerificacao);

            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> ReenviarCodigoAdm(int usuarioId)
        {
            var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);

            if (usuario == null || usuario.Role != "Admin")
            {
                return Json(new { success = false, message = "Usuário administrador não encontrado." });
            }

            string codigoVerificacao = GerarCodigoVerificacao();
            usuario.CodigoValidacao = codigoVerificacao;
            usuario.CodigoValidacaoExpiracao = DateTime.UtcNow.AddMinutes(5);

            await _usuarioRepository.UpdateCodigoVerificacaoAsync(usuario.UsuarioId, codigoVerificacao, usuario.CodigoValidacaoExpiracao);
            await _emailService.EnviarEmailCodigoVerificacaoAsync(usuario.Email, usuario.Nome, codigoVerificacao);

            return Json(new { success = true, message = "Novo código de verificação enviado para o email." });
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Login");
        }

        private string GerarCodigoVerificacao()
        {
            Random random = new Random();
            return random.Next(100000, 999999).ToString();
        }
    }
}
