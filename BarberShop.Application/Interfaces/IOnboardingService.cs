using System.Collections.Generic;
using System.Threading.Tasks;

namespace BarberShop.Application.Interfaces
{
    public interface IOnboardingService
    {
        Task<bool> VerificarProgressoAsync(int usuarioId, string tela);
        Task SalvarProgressoAsync(int usuarioId, string tela);
        Task RegistrarPassosIniciaisAsync(int usuarioId, IEnumerable<string> telas);
        Task<List<string>> ObterTelasPendentesAsync(int usuarioId);
        Task<bool> IsOnboardingComplete(int usuarioId, string tela);

    }
}
