using BarberShop.Application.Interfaces;
using BarberShop.Domain.Entities;
using BarberShop.Domain.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BarberShop.Application.Services
{
    public class FeriadoBarbeariaService : IFeriadoBarbeariaService
    {
        private readonly IFeriadoBarbeariaRepository _feriadoBarbeariaRepository;
        private readonly IFeriadoNacionalRepository _feriadoNacionalRepository;

        public FeriadoBarbeariaService(
            IFeriadoBarbeariaRepository feriadoBarbeariaRepository,
            IFeriadoNacionalRepository feriadoNacionalRepository)
        {
            _feriadoBarbeariaRepository = feriadoBarbeariaRepository;
            _feriadoNacionalRepository = feriadoNacionalRepository;
        }

        public async Task<IEnumerable<FeriadoBarbearia>> ObterFeriadosPorBarbeariaAsync(int barbeariaId)
        {
            return await _feriadoBarbeariaRepository.ObterFeriadosPorBarbeariaAsync(barbeariaId);
        }

        public async Task<IEnumerable<FeriadoNacional>> ObterFeriadosNacionaisAsync()
        {
            return await _feriadoNacionalRepository.ObterTodosFeriadosAsync();
        }

        public async Task<FeriadoBarbearia> ObterPorIdAsync(int id)
        {
            return await _feriadoBarbeariaRepository.GetByIdAsync(id);
        }

        public async Task AdicionarFeriadoAsync(FeriadoBarbearia feriado)
        {
            await _feriadoBarbeariaRepository.AddAsync(feriado);
        }

        public async Task AtualizarFeriadoAsync(FeriadoBarbearia feriado)
        {
            await _feriadoBarbeariaRepository.UpdateAsync(feriado);
        }

        public async Task ExcluirFeriadoAsync(int id)
        {
            await _feriadoBarbeariaRepository.DeleteAsync(id);
        }
    }
}
