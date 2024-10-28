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
        private readonly IEmailService _emailService;

        public LoginController(IClienteRepository clienteRepository, IEmailService emailService)
        {
            _clienteRepository = clienteRepository;
            _emailService = emailService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string phoneInput, string emailInput)
        {
            try
            {
                string userInput = !string.IsNullOrEmpty(phoneInput) ? phoneInput : emailInput;

                if (string.IsNullOrEmpty(userInput))
                {
                    return Json(new { success = false, message = "Por favor, insira um telefone ou email válido." });
                }

                var cliente = await _clienteRepository.GetByEmailOrPhoneAsync(userInput);

                if (cliente != null)
                {
                    // Gerar código de verificação (6 dígitos numéricos)
                    string codigoVerificacao = GerarCodigoVerificacao();

                    // Atualizar apenas os campos de verificação e expiração
                    cliente.CodigoValidacao = codigoVerificacao;
                    cliente.CodigoValidacaoExpiracao = DateTime.UtcNow.AddMinutes(5); // Código válido por 5 minutos

                    await _clienteRepository.UpdateCodigoVerificacaoAsync(cliente.ClienteId, codigoVerificacao, cliente.CodigoValidacaoExpiracao);

                    // Enviar código por email usando o novo método
                    await _emailService.EnviarEmailCodigoVerificacaoAsync(
                        destinatarioEmail: cliente.Email,
                        destinatarioNome: cliente.Nome,
                        codigoVerificacao: codigoVerificacao
                    );

                    // Retornar sucesso e clienteId
                    return Json(new { success = true, clienteId = cliente.ClienteId });
                }
                else
                {
                    return Json(new { success = false, message = "Cliente não encontrado. Revise a informação e tente novamente." });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao fazer login: {ex.Message}");
                return Json(new { success = false, message = "Ocorreu um erro ao tentar fazer login. Por favor, tente novamente mais tarde." });
            }
        }

        // Método para verificar o código
        [HttpPost]
        public async Task<IActionResult> VerificarCodigo(int clienteId, string codigo)
        {
            var cliente = await _clienteRepository.GetByIdAsync(clienteId);

            if (cliente == null)
            {
                return Json(new { success = false, message = "Cliente não encontrado." });
            }

            // Verificar se o código expirou
            if (cliente.CodigoValidacaoExpiracao < DateTime.UtcNow)
            {
                return Json(new { success = false, message = "O código expirou. Por favor, solicite um novo código." });
            }

            // Verificar se o código está correto
            if (codigo == cliente.CodigoValidacao)
            {
                // Autenticar o usuário
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, cliente.ClienteId.ToString()),
                    new Claim(ClaimTypes.Name, cliente.Nome),
                    new Claim(ClaimTypes.Email, cliente.Email ?? cliente.Telefone)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true
                };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                // Limpar o código de verificação
                cliente.CodigoValidacao = null;
                cliente.CodigoValidacaoExpiracao = null;
                await _clienteRepository.UpdateAsync(cliente);

                // Retornar sucesso com a URL de redirecionamento
                return Json(new { success = true, redirectUrl = Url.Action("MenuPrincipal", "Cliente") });
            }
            else
            {
                return Json(new { success = false, message = "Código inválido. Por favor, tente novamente." });
            }
        }

        // Método para reenviar o código
        [HttpGet]
        public async Task<IActionResult> ReenviarCodigo(int clienteId)
        {
            var cliente = await _clienteRepository.GetByIdAsync(clienteId);

            if (cliente == null)
            {
                return Json(new { success = false, message = "Cliente não encontrado." });
            }

            // Gerar novo código de verificação
            string codigoVerificacao = GerarCodigoVerificacao();

            // Atualizar código e expiração
            cliente.CodigoValidacao = codigoVerificacao;
            cliente.CodigoValidacaoExpiracao = DateTime.UtcNow.AddMinutes(5);
            await _clienteRepository.UpdateAsync(cliente);

            // Enviar código por email usando o novo método
            await _emailService.EnviarEmailCodigoVerificacaoAsync(
                destinatarioEmail: cliente.Email,
                destinatarioNome: cliente.Nome,
                codigoVerificacao: codigoVerificacao
            );

            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // Garantir que a requisição seja válida
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Login");
        }



        // Método auxiliar para gerar o código de verificação
        private string GerarCodigoVerificacao()
        {
            Random random = new Random();
            int code = random.Next(100000, 999999); // Gera um número entre 100000 e 999999
            return code.ToString();
        }
    }
}
