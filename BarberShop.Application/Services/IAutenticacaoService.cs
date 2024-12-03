using BarberShop.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BarberShop.Application.Services
{
    public interface IAutenticacaoService
    {
        ClaimsPrincipal AutenticarCliente(Cliente cliente, string barbeariaUrl = null);
        string HashPassword(string senha);
        bool VerifyPassword(string senha, string senhaHash);
    }
}
