using BarberShop.Application.Interfaces;
using BarberShop.Domain.Entities;
using BarberShop.Domain.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BarberShop.Application.Services
{
    public class OnboardingService : IOnboardingService
    {
        private readonly IOnboardingProgressoRepository _onboardingProgressoRepository;

        public OnboardingService(IOnboardingProgressoRepository onboardingProgressoRepository)
        {
            _onboardingProgressoRepository = onboardingProgressoRepository;
        }

        public async Task<bool> VerificarProgressoAsync(int usuarioId, string tela)
        {
            return await _onboardingProgressoRepository.VerificarProgressoAsync(usuarioId, tela);
        }

        public async Task SalvarProgressoAsync(int usuarioId, string tela)
        {
            var progresso = new OnboardingProgresso
            {
                UsuarioId = usuarioId,
                Tela = tela,
                Concluido = true,
                DataConclusao = DateTime.Now
            };

            await _onboardingProgressoRepository.SalvarProgressoAsync(progresso);
        }

        public async Task RegistrarPassosIniciaisAsync(int usuarioId, IEnumerable<string> telas)
        {
            foreach (var tela in telas)
            {
                var progressoExistente = await _onboardingProgressoRepository.VerificarProgressoAsync(usuarioId, tela);
                if (!progressoExistente)
                {
                    var novoProgresso = new OnboardingProgresso
                    {
                        UsuarioId = usuarioId,
                        Tela = tela,
                        Concluido = false
                    };

                    await _onboardingProgressoRepository.SalvarProgressoAsync(novoProgresso);
                }
            }
        }

        public async Task<List<string>> ObterTelasPendentesAsync(int usuarioId)
        {
            return await _onboardingProgressoRepository.ObterTelasPendentesAsync(usuarioId);
        }

        public async Task<bool> IsOnboardingComplete(int usuarioId, string tela)
        {
            // Verifica se o progresso está concluído no repositório
            return await _onboardingProgressoRepository.VerificarProgressoAsync(usuarioId, tela);
        }
    }
}
