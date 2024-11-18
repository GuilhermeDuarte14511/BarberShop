using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberShop.Domain.Entities
{
    public class FeriadoNacional
    {
        public int FeriadoId { get; set; }
        public DateTime Data { get; set; }
        public string Descricao { get; set; }
        public bool Recorrente { get; set; }
    }
}
