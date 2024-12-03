using BarberShop.Domain.Entities;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace BarberShop.Application.Services
{
    public class AutenticacaoService : IAutenticacaoService
    {
        public ClaimsPrincipal AutenticarCliente(Cliente cliente, string barbeariaUrl)
        {
            if (cliente == null)
            {
                return null;
            }

            // Criando os claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, cliente.ClienteId.ToString()),
                new Claim(ClaimTypes.Name, cliente.Nome),
                new Claim(ClaimTypes.Email, cliente.Email ?? cliente.Telefone),
                new Claim("Telefone", cliente.Telefone ?? string.Empty),
                new Claim(ClaimTypes.Role, cliente.Role),
                new Claim("BarbeariaUrl", barbeariaUrl) // Adiciona o UrlSlug da barbearia
            };

            // Configurando a identidade e principal
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            return new ClaimsPrincipal(claimsIdentity);
        }


        public string HashPassword(string senha)
        {
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(senha));
            return Convert.ToHexString(hashBytes); // Método moderno para converter bytes em string hexadecimal
        }

        public bool VerifyPassword(string senha, string senhaHash)
        {
            var hashedPassword = HashPassword(senha);
            return hashedPassword == senhaHash;
        }
    }
}