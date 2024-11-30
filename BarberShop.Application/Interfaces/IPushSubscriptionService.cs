using BarberShop.Application.DTOs;

namespace BarberShop.Application.Interfaces
{
    public interface IPushSubscriptionService
    {
        void SalvarInscricao(PushSubscriptionDTO subscriptionDTO);
        IEnumerable<PushSubscriptionDTO> ObterInscricoesPorUsuario(int usuarioId);

    }
}
