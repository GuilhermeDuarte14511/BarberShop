using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberShop.Application.DTOs
{
    public class PushSubscriptionDTO
    {
        public int Id { get; set; }
        public int? UsuarioId { get; set; } // ID do usuário associado
        public string Endpoint { get; set; }
        public string P256dh { get; set; }
        public string Auth { get; set; }
        public DateTime DataCadastro { get; set; }

    }
}
