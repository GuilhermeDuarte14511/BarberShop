using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberShop.Domain.Entities
{
    public class DadosCartao
    {
        public string Numero { get; set; }
        public string Titular { get; set; }
        public string Validade { get; set; }
        public string CVV { get; set; }
    }

}
