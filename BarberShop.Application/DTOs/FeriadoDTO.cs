using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberShop.Application.DTOs
{
    public class FeriadoDTO
    {
        public string Descricao { get; set; }
        public DateTime Data { get; set; }
        public bool Recorrente { get; set; }
        public int? FeriadoBarbeariaId { get; set; } // Nulo para feriados nacionais
        public bool Fixo { get; set; } // True para feriados nacionais, False para personalizáveis
    }

}
