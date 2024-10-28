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

        public LoginController(IClienteRepository clienteRepository)
        {
            _clienteRepository = clienteRepository;
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
                string userInput = string.IsNullOrEmpty(phoneInput) ? emailInput : phoneInput;

                if (string.IsNullOrEmpty(userInput))
                {
                    TempData["LoginError"] = "Por favor, insira um telefone ou email válido.";
                    return View();
                }

                var cliente = await _clienteRepository.GetByEmailOrPhoneAsync(userInput);

                if (cliente != null)
                {
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

                    // Tenta autenticar o usuário
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                    return RedirectToAction("MenuPrincipal", "Cliente");
                }
                else
                {
                    TempData["LoginError"] = "Cliente não encontrado. Revise a informação e tente novamente.";
                    return View();
                }
            }
            catch (Exception ex)
            {
                // Captura qualquer exceção, como erros de banco de dados
                TempData["LoginError"] = "Ocorreu um erro ao tentar fazer login. Por favor, tente novamente mais tarde.";
                // Aqui, opcionalmente, você pode logar o erro usando algum sistema de logging
                // ex: _logger.LogError(ex, "Erro ao tentar fazer login");
                // TempData["LoginError"] = ex.Message; // Para mostrar o erro exato durante o desenvolvimento
                return View();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            try
            {
                // Tenta fazer o logout
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                // Em caso de erro, redireciona para a página de login com uma mensagem de erro
                TempData["LoginError"] = "Ocorreu um erro ao tentar realizar o logout. Por favor, tente novamente.";
                return RedirectToAction("Login");
            }
        }
    }
}
