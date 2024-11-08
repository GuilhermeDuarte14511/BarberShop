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

        // Novo método para obter barbeiros por barbeariaId
        public async Task<IEnumerable<Barbeiro>> ObterBarbeirosPorBarbeariaIdAsync(int barbeariaId)
        {
            return await _barbeiroRepository.GetAllByBarbeariaIdAsync(barbeariaId);
        }


        public async Task<IEnumerable<DateTime>> ObterHorariosDisponiveisAsync(int barbeiroId, int? duracaoTotal)
        {
            if (!duracaoTotal.HasValue)
            {
                throw new ArgumentException("A duração total é necessária", nameof(duracaoTotal));
            }

            DateTime horarioInicio = DateTime.Today.AddHours(9); // 09:00
            DateTime horarioFim = DateTime.Today.AddHours(18);   // 18:00

            // Obtém os agendamentos do barbeiro para o dia atual
            var agendamentos = await _agendamentoRepository.ObterAgendamentosPorBarbeiroIdAsync(barbeiroId, DateTime.Today);

            // Mapeia os horários ocupados considerando a duração de cada agendamento
            var horariosOcupados = agendamentos.Select(a => new
            {
                Inicio = a.DataHora,
                Fim = a.DataHora.AddMinutes(a.DuracaoTotal ?? 0) // Usa 0 como valor padrão se DuracaoTotal for nulo
            }).ToList();

            List<DateTime> horariosDisponiveis = new List<DateTime>();
            int duracao = duracaoTotal.Value; // Obtém o valor de duracaoTotal com certeza de que não é nulo

            // Itera pelos horários do expediente, pulando intervalos ocupados
            while (horarioInicio.AddMinutes(duracao) <= horarioFim)
            {
                bool horarioConflitante = horariosOcupados.Any(a =>
                    (horarioInicio >= a.Inicio && horarioInicio < a.Fim) ||
                    (horarioInicio.AddMinutes(duracao) > a.Inicio && horarioInicio.AddMinutes(duracao) <= a.Fim));

                if (!horarioConflitante)
                {
                    horariosDisponiveis.Add(horarioInicio);
                }

                horarioInicio = horarioInicio.AddMinutes(duracao);
            }

            return horariosDisponiveis;

        }

        // Novo método no BarbeiroService para verificar disponibilidade de horário usando o repositório diretamente
        public async Task<bool> VerificarDisponibilidadeHorarioAsync(int barbeiroId, DateTime dataHora, int duracao)
        {
            return await _agendamentoRepository.VerificarDisponibilidadeHorarioAsync(barbeiroId, dataHora, duracao);
        }
    }
}
