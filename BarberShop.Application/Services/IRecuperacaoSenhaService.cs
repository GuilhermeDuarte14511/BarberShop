using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberShop.Application.Services
{
    public interface IRecuperacaoSenhaService
    {
        Task EnviarEmailRecuperacaoSenhaAsync(string email);
        Task<bool> VerificarTokenAsync(int usuarioId, string token);
        Task<bool> RedefinirSenhaAsync(int usuarioId, string novaSenha);
    }
}
