using BarberShop.Application.DTOs;
using BarberShop.Application.Interfaces;
using BarberShop.Domain.Entities;
using BarberShop.Infrastructure.Data;

namespace BarberShop.Application.Services
{
    public class PushSubscriptionService : IPushSubscriptionService
    {
        private readonly BarbeariaContext _context;

        public PushSubscriptionService(BarbeariaContext context)
        {
            _context = context;
        }

        public void SalvarInscricao(PushSubscriptionDTO subscriptionDTO)
        {
            var subscription = new PushSubscription
            {
                UsuarioId = subscriptionDTO.UsuarioId,
                Endpoint = subscriptionDTO.Endpoint,
                P256dh = subscriptionDTO.P256dh,
                Auth = subscriptionDTO.Auth,
                DataCadastro = DateTime.UtcNow
            };

            _context.PushSubscriptions.Add(subscription);
            _context.SaveChanges();
        }

        public IEnumerable<PushSubscriptionDTO> ObterInscricoesPorUsuario(int usuarioId)
        {
            return _context.PushSubscriptions
                .Where(p => p.UsuarioId == usuarioId)
                .Select(p => new PushSubscriptionDTO
                {
                    Id = p.Id,
                    UsuarioId = p.UsuarioId,
                    Endpoint = p.Endpoint,
                    P256dh = p.P256dh,
                    Auth = p.Auth,
                    DataCadastro = p.DataCadastro
                })
                .ToList();
        }



    }
}
