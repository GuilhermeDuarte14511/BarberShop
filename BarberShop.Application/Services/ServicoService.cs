using BarberShop.Application.Interfaces;
using BarberShop.Domain.Entities;
using BarberShop.Domain.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BarberShop.Application.Services
{
    public class ServicoService : IServicoService
    {
        private readonly IServicoRepository _servicoRepository;

        public ServicoService(IServicoRepository servicoRepository)
        {
            _servicoRepository = servicoRepository;
        }

        public async Task<IEnumerable<Servico>> ObterTodosPorBarbeariaIdAsync(int barbeariaId)
        {
            return await _servicoRepository.ObterServicosPorBarbeariaIdAsync(barbeariaId);
        }

        public async Task<Servico> ObterPorIdAsync(int id)
        {
            return await _servicoRepository.GetByIdAsync(id);
        }

        public async Task AdicionarAsync(Servico servico)
        {
            await _servicoRepository.AddAsync(servico);
            await _servicoRepository.SaveChangesAsync();
        }

        public async Task AtualizarAsync(Servico servico)
        {
            await _servicoRepository.UpdateAsync(servico);
            await _servicoRepository.SaveChangesAsync();
        }

        public async Task ExcluirAsync(int id)
        {
            await _servicoRepository.DeleteAsync(id);
            await _servicoRepository.SaveChangesAsync();
        }
    }
}
