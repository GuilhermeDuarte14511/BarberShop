﻿using BarberShop.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BarberShop.Application.Services
{
    public interface IAgendamentoService
    {
        Task<IEnumerable<DateTime>> ObterHorariosDisponiveisAsync(int barbeiroId, DateTime data, int duracaoTotal);
        Task<Agendamento> ObterAgendamentoPorIdAsync(int id);
        Task<IEnumerable<Servico>> ObterServicosAsync();

        Task<int> CriarAgendamentoAsync(int barbeiroId, DateTime dataHora, int clienteId, List<int> servicoIds);
    }
}
