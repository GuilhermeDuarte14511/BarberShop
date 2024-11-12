using BarberShop.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberShop.Application.Services
{
    public interface IPlanoAssinaturaService
    {
        Task<List<PlanoAssinaturaSistema>> GetAllPlanosAsync();
        Task<List<PlanoAssinaturaSistema>> SincronizarPlanosComStripe();
    }
}
