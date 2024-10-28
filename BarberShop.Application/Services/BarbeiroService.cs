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

        public async Task<IEnumerable<DateTime>> ObterHorariosDisponiveisAsync(int barbeiroId, int duracaoTotal)
        {
            // Vamos assumir que o expediente do barbeiro é das 09:00 às 18:00
            DateTime horarioInicio = DateTime.Today.AddHours(9); // 09:00
            DateTime horarioFim = DateTime.Today.AddHours(18);  // 18:00

            // Obtenha todos os agendamentos existentes para o barbeiro no dia atual
            var agendamentos = await _agendamentoRepository.ObterAgendamentosPorBarbeiroIdAsync(barbeiroId, DateTime.Today);

            // Lista de horários ocupados
            var horariosOcupados = agendamentos.Select(a => new
            {
                Inicio = a.DataHora,
                Fim = a.DataHora.AddMinutes(a.DuracaoTotal)
            }).ToList();

            // Lista para armazenar os horários disponíveis
            List<DateTime> horariosDisponiveis = new List<DateTime>();

            // Percorre os horários entre o início e o fim do expediente
            while (horarioInicio.AddMinutes(duracaoTotal) <= horarioFim)
            {
                // Verificar se o horário está ocupado
                bool horarioConflitante = horariosOcupados.Any(a =>
                    (horarioInicio >= a.Inicio && horarioInicio < a.Fim) ||
                    (horarioInicio.AddMinutes(duracaoTotal) > a.Inicio && horarioInicio.AddMinutes(duracaoTotal) <= a.Fim));

                // Se não houver conflitos, adiciona à lista de horários disponíveis
                if (!horarioConflitante)
                {
                    horariosDisponiveis.Add(horarioInicio);
                }

                // Avança para o próximo possível horário
                horarioInicio = horarioInicio.AddMinutes(duracaoTotal);
            }

            return horariosDisponiveis;
        }
    }
}
