using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberShop.Domain.Entities
{
    public class PlanoAssinaturaSistema
    {
        public int PlanoId { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public string IdProdutoStripe { get; set; } // ID do plano na Stripe
        public decimal Valor { get; set; }
        public string Periodicidade { get; set; } // Ex.: "Mensal", "Anual"
        public string PriceId { get; set; } // ID do preço na Stripe


    }
}
