using BarberShop.Application.Interfaces;
using BarberShop.Domain.Entities;
using BarberShop.Domain.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BarberShop.Application.Services
{
    public class BarbeiroServicoService : IBarbeiroServicoService
    {
        private readonly IBarbeiroServicoRepository _barbeiroServicoRepository;

        public BarbeiroServicoService(IBarbeiroServicoRepository barbeiroServicoRepository)
        {
            _barbeiroServicoRepository = barbeiroServicoRepository;
        }

        public async Task<IEnumerable<Servico>> ObterServicosPorBarbeiroIdAsync(int barbeiroId)
        {
            return await _barbeiroServicoRepository.ObterServicosPorBarbeiroIdAsync(barbeiroId);
        }

        public async Task<IEnumerable<Servico>> ObterServicosNaoVinculadosAsync(int barbeiroId)
        {
            return await _barbeiroServicoRepository.ObterServicosNaoVinculadosAsync(barbeiroId);
        }

        public async Task VincularServicoAsync(int barbeiroId, int servicoId)
        {
            await _barbeiroServicoRepository.VincularServicoAsync(barbeiroId, servicoId);
        }

        public async Task<bool> DesvincularServicoAsync(int barbeiroId, int servicoId)
        {
            return await _barbeiroServicoRepository.DesvincularServicoAsync(barbeiroId, servicoId);
        }

        public async Task<Servico> ObterServicoPorIdAsync(int servicoId) // Implementação
        {
            return await _barbeiroServicoRepository.ObterServicoPorIdAsync(servicoId);
        }
    }
}
