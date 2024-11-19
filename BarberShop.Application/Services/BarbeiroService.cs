﻿using BarberShop.Domain.Entities;
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
        private readonly IServicoRepository _servicoRepository; // Adicionado repositório de serviços

        public BarbeiroService(IBarbeiroRepository barbeiroRepository, IAgendamentoRepository agendamentoRepository, IServicoRepository servicoRepository)
        {
            _barbeiroRepository = barbeiroRepository;
            _agendamentoRepository = agendamentoRepository;
            _servicoRepository = servicoRepository; // Injeção do repositório de serviços
        }

        public async Task<IEnumerable<Barbeiro>> ObterTodosBarbeirosAsync()
        {
            return await _barbeiroRepository.GetAllAsync();
        }

        public async Task<Barbeiro> ObterBarbeiroPorIdAsync(int id)
        {
            return await _barbeiroRepository.GetByIdAsync(id);
        }

        public async Task<Barbeiro> VerificarExistenciaPorEmailOuTelefoneAsync(string email, string telefone)
        {
            return await _barbeiroRepository.GetByEmailOrPhoneAsync(email, telefone);
        }

        public async Task<IEnumerable<Barbeiro>> ObterBarbeirosPorBarbeariaIdAsync(int barbeariaId)
        {
            return await _barbeiroRepository.GetAllByBarbeariaIdAsync(barbeariaId);
        }

        public async Task<IEnumerable<DateTime>> ObterHorariosDisponiveisAsync(int barbeiroId, int? duracaoTotal)
        {
            if (!duracaoTotal.HasValue)
                throw new ArgumentException("A duração total é necessária", nameof(duracaoTotal));

            DateTime horarioInicio = DateTime.Today.AddHours(9); // 09:00
            DateTime horarioFim = DateTime.Today.AddHours(18);   // 18:00

            var agendamentos = await _agendamentoRepository.ObterAgendamentosPorBarbeiroIdAsync(barbeiroId, DateTime.Today);

            var horariosOcupados = agendamentos.Select(a => new
            {
                Inicio = a.DataHora,
                Fim = a.DataHora.AddMinutes(a.DuracaoTotal ?? 0) // Usa 0 como padrão se DuracaoTotal for nulo
            }).ToList();

            List<DateTime> horariosDisponiveis = new List<DateTime>();
            int duracao = duracaoTotal.Value;

            while (horarioInicio.AddMinutes(duracao) <= horarioFim)
            {
                bool horarioConflitante = horariosOcupados.Any(a =>
                    (horarioInicio >= a.Inicio && horarioInicio < a.Fim) ||
                    (horarioInicio.AddMinutes(duracao) > a.Inicio && horarioInicio.AddMinutes(duracao) <= a.Fim));

                if (!horarioConflitante)
                    horariosDisponiveis.Add(horarioInicio);

                horarioInicio = horarioInicio.AddMinutes(duracao);
            }

            return horariosDisponiveis;
        }

        public async Task<bool> VerificarDisponibilidadeHorarioAsync(int barbeiroId, DateTime dataHora, int duracao)
        {
            return await _agendamentoRepository.VerificarDisponibilidadeHorarioAsync(barbeiroId, dataHora, duracao);
        }

        public async Task<IEnumerable<Barbeiro>> ObterBarbeirosPorServicosAsync(int barbeariaId, List<int> servicoIds)
        {
            var barbeiros = await _barbeiroRepository.GetAllByBarbeariaIdAsync(barbeariaId);
            var barbeirosServicos = await _barbeiroRepository.ObterBarbeirosComServicosPorBarbeariaIdAsync(barbeariaId);

            foreach (var barbeiro in barbeiros)
            {
                var servicosNaoRealizados = servicoIds
                    .Where(servicoId => !barbeirosServicos.Any(bs => bs.BarbeiroId == barbeiro.BarbeiroId && bs.ServicoId == servicoId))
                    .ToList();

                if (servicosNaoRealizados.Any())
                {
                    var servicosNomes = await _servicoRepository.ObterNomesServicosAsync(servicosNaoRealizados);
                    barbeiro.ServicosNaoRealizados.AddRange(servicosNomes);
                }
            }

            return barbeiros;
        }
    }
}
