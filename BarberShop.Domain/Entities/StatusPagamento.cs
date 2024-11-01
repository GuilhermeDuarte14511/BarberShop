using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberShop.Domain.Entities
{
    public enum StatusPagamento
    {
        NãoEspecificado = -1, // Representa um valor NULL no banco de dados
        Pendente = 0,         // Pagamento ainda não foi processado
        Aprovado = 1,         // Pagamento aprovado
        Recusado = 2,         // Pagamento foi recusado
        Cancelado = 3,        // Pagamento foi cancelado
        Reembolsado = 4,      // Pagamento foi reembolsado
        EmProcessamento = 5   // Pagamento está em processamento
    }

}
