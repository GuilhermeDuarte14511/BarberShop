using BarberShop.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BarberShop.Domain.Interfaces
{
    public interface IOnboardingProgressoRepository
    {
        Task<bool> VerificarProgressoAsync(int usuarioId, string tela);
        Task SalvarProgressoAsync(OnboardingProgresso progresso);
        Task<List<OnboardingProgresso>> ObterTodosAsync();
        Task<List<string>> ObterTelasPendentesAsync(int usuarioId);

    }
}
