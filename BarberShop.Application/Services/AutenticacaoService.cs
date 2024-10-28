using BarberShop.Domain.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace BarberShop.Application.Services
{
    public class AutenticacaoService
    {
        private readonly IClienteRepository _clienteRepository;

        public AutenticacaoService(IClienteRepository clienteRepository)
        {
            _clienteRepository = clienteRepository;
        }

        public async Task<ClaimsPrincipal> AutenticarClienteAsync(string emailOuTelefone)
        {
            var cliente = await _clienteRepository.GetByEmailOrPhoneAsync(emailOuTelefone);
            if (cliente == null)
            {
                return null;
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, cliente.ClienteId.ToString()),
                new Claim(ClaimTypes.Name, cliente.Nome),
                new Claim(ClaimTypes.Email, cliente.Email ?? cliente.Telefone)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            return new ClaimsPrincipal(claimsIdentity);
        }
    }
}
