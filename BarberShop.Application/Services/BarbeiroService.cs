using BarberShop.Domain.Entities;
using BarberShop.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BarberShop.Application.Services
{
    public class BarbeiroService : IBarbeiroService
    {
        private readonly IBarbeiroRepository _barbeiroRepository;
        private readonly IAgendamentoRepository _agendamentoRepository;

        public BarbeiroService(IBarbeiroRepository barbeiroRepository, IAgendamentoRepository agendamentoRepository)
        {
            _barbeiroRepository = barbeiroRepository;
            _agendamentoRepository = agendamentoRepository;
        }

        public async Task<IEnumerable<Barbeiro>> ObterTodosBarbeirosAsync()
        {
            return await _barbeiroRepository.GetAllAsync();
        }

        public async Task<Barbeiro> ObterBarbeiroPorIdAsync(int id)
        {
            return await _barbeiroRepository.GetByIdAsync(id);
        }

        // Implementação do novo método para verificar existência por email ou telefone
        public async Task<Barbeiro> VerificarExistenciaPorEmailOuTelefoneAsync(string email, string telefone)
        {
            return await _barbeiroRepository.GetByEmailOrPhoneAsync(email, telefone);
        }

        public async Task<IEnumerable<DateTime>> ObterHorariosDisponiveisAsync(int barbeiroId, int duracaoTotal)
        {
            DateTime horarioInicio = DateTime.Today.AddHours(9); // 09:00
            DateTime horarioFim = DateTime.Today.AddHours(18);  // 18:00

            var agendamentos = await _agendamentoRepository.ObterAgendamentosPorBarbeiroIdAsync(barbeiroId, DateTime.Today);

            var horariosOcupados = agendamentos.Select(a => new
            {
                Inicio = a.DataHora,
                Fim = a.DataHora.AddMinutes(a.DuracaoTotal)
            }).ToList();

            List<DateTime> horariosDisponiveis = new List<DateTime>();

            while (horarioInicio.AddMinutes(duracaoTotal) <= horarioFim)
            {
                bool horarioConflitante = horariosOcupados.Any(a =>
                    (horarioInicio >= a.Inicio && horarioInicio < a.Fim) ||
                    (horarioInicio.AddMinutes(duracaoTotal) > a.Inicio && horarioInicio.AddMinutes(duracaoTotal) <= a.Fim));

                if (!horarioConflitante)
                {
                    horariosDisponiveis.Add(horarioInicio);
                }

                horarioInicio = horarioInicio.AddMinutes(duracaoTotal);
            }

            return horariosDisponiveis;
        }
    }
}
