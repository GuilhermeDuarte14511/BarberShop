using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberShop.Domain.Entities
{
    public class GraficoPosicao
    {
        public int? Id { get; set; }
        public int? UsuarioId { get; set; }
        public string? GraficoId { get; set; }
        public int? Posicao { get; set; }
    }

}
