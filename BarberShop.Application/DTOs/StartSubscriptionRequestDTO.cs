using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberShop.Application.DTOs
{
    public class StartSubscriptionRequestDTO
    {
        public string PlanId { get; set; }
        public string ClienteNome { get; set; }
        public string ClienteEmail { get; set; }
        public string PriceId { get; set; } // Nova propriedade

    }
}
