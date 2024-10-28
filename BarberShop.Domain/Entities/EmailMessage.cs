using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberShop.Domain.Entities
{
    public class EmailMessage
    {
        public string NomeCliente { get; set; }
        public string EmailCliente { get; set; }
        public string NomeBarbeiro { get; set; }
        public string EmailBarbeiro { get; set; }
        public string ConteudoEmailCliente { get; set; }
        public string ConteudoEmailBarbeiro { get; set; }
        public string GoogleCalendarLink { get; set; }

        // Novas propriedades adicionadas
        public DateTime DataHoraInicio { get; set; }
        public DateTime DataHoraFim { get; set; }
        public decimal ValorTotal { get; set; }
    }
}
