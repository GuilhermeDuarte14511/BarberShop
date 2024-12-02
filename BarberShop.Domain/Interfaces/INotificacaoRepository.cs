using BarberShop.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberShop.Domain.Interfaces
{
    public interface INotificacaoRepository
    {
        IEnumerable<Notificacao> ObterPorUsuario(int usuarioId);
        IEnumerable<Agendamento> ObterAgendamentosDoDia(DateTime data);
        Usuario ObterBarbeiroPorId(int barbeiroId);
        IEnumerable<Usuario> ObterAdminsPorBarbearia(int barbeariaId);
        IEnumerable<Usuario> ObterBarbeirosPorBarbearia(int barbeariaId); // Adicionado
        Notificacao ObterUltimaNotificacao(int usuarioId, int agendamentoId, DateTime referencia);
        void Criar(Notificacao notificacao);
        void SalvarAlteracoes();
        IEnumerable<Notificacao> ObterNotificacoes(int usuarioId, int agendamentoId);
        Barbearia ObterBarbeariaPorId(int barbeariaId);
        Agendamento ObterAgendamentoPorId(int agendamentoId);


    }


}
