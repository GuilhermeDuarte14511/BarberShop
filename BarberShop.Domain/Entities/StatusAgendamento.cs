using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberShop.Domain.Entities
{
    public enum StatusAgendamento
    {
        Pendente = 0,
        Confirmado = 1,
        Cancelado = 2,
        Concluido = 3
    }
}
