using BarberShop.Application.Interfaces;
using BarberShop.Domain.Entities;
using BarberShop.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BarberShop.Application.Services
{
    public class IndisponibilidadeService : IIndisponibilidadeService
    {
        private readonly IIndisponibilidadeRepository _indisponibilidadeRepository;

        public IndisponibilidadeService(IIndisponibilidadeRepository indisponibilidadeRepository)
        {
            _indisponibilidadeRepository = indisponibilidadeRepository;
        }

        // Obtém as indisponibilidades de um barbeiro (entidades completas)
        public async Task<IEnumerable<IndisponibilidadeBarbeiro>> ObterIndisponibilidadesPorBarbeiroAsync(int barbeiroId)
        {
            return await _indisponibilidadeRepository.ObterIndisponibilidadesPorBarbeiroAsync(barbeiroId);
        }

        // Obtém as indisponibilidades de uma barbearia
        public async Task<IEnumerable<IndisponibilidadeBarbeiro>> ObterIndisponibilidadesPorBarbeariaAsync(int barbeariaId)
        {
            return await _indisponibilidadeRepository.GetByBarbeariaIdAsync(barbeariaId);
        }

        // Obtém uma indisponibilidade por ID
        public async Task<IndisponibilidadeBarbeiro> ObterPorIdAsync(int id)
        {
            return await _indisponibilidadeRepository.GetByIdAsync(id);
        }

        public async Task AdicionarIndisponibilidadeAsync(IndisponibilidadeBarbeiro indisponibilidade)
        {
            if (indisponibilidade == null)
                throw new ArgumentNullException(nameof(indisponibilidade), "A indisponibilidade não pode ser nula.");

            await _indisponibilidadeRepository.AddAsync(indisponibilidade);
            await _indisponibilidadeRepository.SaveChangesAsync(); // Explicitamente salva as mudanças
        }


        // Atualiza uma indisponibilidade existente
        public async Task AtualizarIndisponibilidadeAsync(IndisponibilidadeBarbeiro indisponibilidade)
        {
            if (indisponibilidade == null)
                throw new ArgumentNullException(nameof(indisponibilidade), "A indisponibilidade não pode ser nula.");

            await _indisponibilidadeRepository.UpdateAsync(indisponibilidade);
        }

        // Exclui uma indisponibilidade por ID
        public async Task ExcluirIndisponibilidadeAsync(int id)
        {
            await _indisponibilidadeRepository.DeleteAsync(id);
        }
    }
}
