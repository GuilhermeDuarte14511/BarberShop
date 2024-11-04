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

        public LoginController(IClienteRepository clienteRepository, IUsuarioRepository usuarioRepository, IEmailService emailService, AutenticacaoService autenticacaoService)
        {
            _clienteRepository = clienteRepository;
            _usuarioRepository = usuarioRepository;
            _emailService = emailService;
            _autenticacaoService = autenticacaoService;
        }

        // Exibe a página de login para clientes
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // Exibe a página de login administrativo
        [HttpGet]
        public IActionResult AdminLogin()
        {
            return View();
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

            // Gerar e enviar código de verificação por email
            string codigoVerificacao = GerarCodigoVerificacao();
            usuario.CodigoValidacao = codigoVerificacao;
            usuario.CodigoValidacaoExpiracao = DateTime.UtcNow.AddMinutes(5);
            await _usuarioRepository.UpdateCodigoVerificacaoAsync(usuario.UsuarioId, codigoVerificacao, usuario.CodigoValidacaoExpiracao);
            await _emailService.EnviarEmailCodigoVerificacaoAsync(usuario.Email, usuario.Nome, codigoVerificacao);

            return Json(new { success = true, usuarioId = usuario.UsuarioId });
        }

        // Verifica o código de verificação para login administrativo
        [HttpPost]
        public async Task<IActionResult> VerificarAdminCodigo(int usuarioId, string codigo)
        {
            var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);

            if (usuario == null || usuario.CodigoValidacaoExpiracao < DateTime.UtcNow || codigo != usuario.CodigoValidacao)
            {
                return Json(new { success = false, message = "Código inválido ou expirado." });
            }

            // Gerar claims do usuário para autenticação
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.UsuarioId.ToString()),
                new Claim(ClaimTypes.Name, usuario.Nome),
                new Claim(ClaimTypes.Role, usuario.Role) // Adiciona a role do usuário
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            var authProperties = new AuthenticationProperties { IsPersistent = true };
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal, authProperties);

            // Redirecionar para a área administrativa
            return Json(new { success = true, redirectUrl = Url.Action("Index", "Admin") });
        }

        // Processa o login para clientes
        [HttpPost]
        public async Task<IActionResult> Login(string phoneInput, string emailInput)
        {
            string userInput = !string.IsNullOrEmpty(phoneInput) ? phoneInput : emailInput;

            if (string.IsNullOrEmpty(userInput))
            {
                return Json(new { success = false, message = "Por favor, insira um telefone ou email válido." });
            }

            var cliente = await _clienteRepository.GetByEmailOrPhoneAsync(userInput);

            if (cliente != null)
            {
                string codigoVerificacao = GerarCodigoVerificacao();
                cliente.CodigoValidacao = codigoVerificacao;
                cliente.CodigoValidacaoExpiracao = DateTime.UtcNow.AddMinutes(5);

                await _clienteRepository.UpdateCodigoVerificacaoAsync(cliente.ClienteId, codigoVerificacao, cliente.CodigoValidacaoExpiracao);
                await _emailService.EnviarEmailCodigoVerificacaoAsync(cliente.Email, cliente.Nome, codigoVerificacao);

                return Json(new { success = true, clienteId = cliente.ClienteId });
            }
            else
            {
                return Json(new { success = false, message = "Cliente não encontrado. Revise a informação e tente novamente." });
            }
        }

        // Processa o cadastro de novos clientes
        [HttpPost]
        public async Task<IActionResult> Cadastro(string nameInput, string registerEmailInput, string registerPhoneInput)
        {
            if (string.IsNullOrEmpty(registerEmailInput) || string.IsNullOrEmpty(registerPhoneInput) || string.IsNullOrEmpty(nameInput))
            {
                return Json(new { success = false, message = "Todos os campos são obrigatórios." });
            }

            var clienteExistente = await _clienteRepository.GetByEmailOrPhoneAsync(registerEmailInput);
            if (clienteExistente != null)
            {
                return Json(new { success = false, message = "Este email já está cadastrado." });
            }

            var cliente = new Cliente
            {
                Nome = nameInput,
                Email = registerEmailInput,
                Telefone = registerPhoneInput,
                CodigoValidacao = GerarCodigoVerificacao(),
                CodigoValidacaoExpiracao = DateTime.UtcNow.AddMinutes(5),
                Role = "Cliente" // Novo cliente será do tipo Cliente por padrão
            };

            await _clienteRepository.AddAsync(cliente);
            await _emailService.EnviarEmailCodigoVerificacaoAsync(cliente.Email, cliente.Nome, cliente.CodigoValidacao);

            return Json(new { success = true, clienteId = cliente.ClienteId });
        }

        // Verifica o código de verificação para clientes
        [HttpPost]
        public async Task<IActionResult> VerificarCodigo(int clienteId, string codigo)
        {
            var cliente = await _clienteRepository.GetByIdAsync(clienteId);

            if (cliente == null || cliente.CodigoValidacaoExpiracao < DateTime.UtcNow || codigo != cliente.CodigoValidacao)
            {
                return Json(new { success = false, message = "Código inválido ou expirado." });
            }

            // Gerar claims do usuário para autenticação
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, cliente.ClienteId.ToString()),
                new Claim(ClaimTypes.Name, cliente.Nome),
                new Claim(ClaimTypes.Role, cliente.Role) // Adiciona a role do usuário
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            var authProperties = new AuthenticationProperties { IsPersistent = true };
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal, authProperties);

            // Redirecionar para a área administrativa se for administrador
            var redirectUrl = cliente.Role == "Admin" ? Url.Action("Index", "Admin") : Url.Action("MenuPrincipal", "Cliente");

            // Atualizando o cliente para manter registros do código
            await _clienteRepository.UpdateAsync(cliente);

            return Json(new { success = true, redirectUrl });
        }

        // Reenvio do código de verificação para clientes
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

        // Reenvio do código de verificação para login administrativo
        [HttpGet]
        public async Task<IActionResult> ReenviarCodigoAdm(int usuarioId)
        {
            var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);

            if (usuario == null || usuario.Role != "Admin")
            {
                return Json(new { success = false, message = "Usuário administrador não encontrado." });
            }

            // Gerar e enviar novo código de verificação
            string codigoVerificacao = GerarCodigoVerificacao();
            usuario.CodigoValidacao = codigoVerificacao;
            usuario.CodigoValidacaoExpiracao = DateTime.UtcNow.AddMinutes(5);

            await _usuarioRepository.UpdateCodigoVerificacaoAsync(usuario.UsuarioId, codigoVerificacao, usuario.CodigoValidacaoExpiracao);
            await _emailService.EnviarEmailCodigoVerificacaoAsync(usuario.Email, usuario.Nome, codigoVerificacao);

            return Json(new { success = true, message = "Novo código de verificação enviado para o email." });
        }

        // Logout
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Login");
        }

        // Geração de código de verificação
        private string GerarCodigoVerificacao()
        {
            Random random = new Random();
            return random.Next(100000, 999999).ToString();
        }
    }
}
