using BarberShop.Application.DTOs;
using BarberShop.Domain.Entities;
using System.Collections.Generic;

namespace BarberShop.Application.Interfaces
{
    public interface INotificacaoService
    {
        void NotificarAdmins(int barbeariaId, Agendamento agendamento); // Notifica todos os admins
        void NotificarBarbeiro(Agendamento agendamento); // Notifica o barbeiro específico
        void GerarNotificacoesDeAgendamentosProximos(); // Gera notificações periódicas para admins e barbeiros
        IEnumerable<NotificacaoDTO> ObterPorUsuario(int usuarioId); // Busca notificações de um usuário
        IEnumerable<object> ObterNotificacoesAgrupadasPorDia(int usuarioId); // Agrupa notificações por dia
        void MarcarTodasComoLidas(int usuarioId); // Marca todas as notificações como lidas
        void CriarNotificacao(NotificacaoDTO notificacao); // Cria uma nova notificação
        void NotificarAvaliacaoRecebida(Avaliacao avaliacao);

    }
}
