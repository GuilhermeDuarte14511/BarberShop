using BarberShop.Domain.Entities;
using BarberShop.Application.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace BarberShop.Application.Services
{
    public class AutenticacaoService : IAutenticacaoService
    {
        public ClaimsPrincipal AutenticarCliente(Cliente cliente)
        {
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

        public string HashPassword(string senha)
        {
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(senha));
            var hashedPassword = new StringBuilder();
            foreach (var b in hashBytes)
            {
                hashedPassword.Append(b.ToString("x2"));
            }
            return hashedPassword.ToString();
        }

        public bool VerifyPassword(string senha, string senhaHash)
        {
            var hashedPassword = HashPassword(senha);
            return hashedPassword == senhaHash;
        }
    }
}
