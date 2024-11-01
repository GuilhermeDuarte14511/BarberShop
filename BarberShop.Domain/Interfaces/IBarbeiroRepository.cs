﻿using BarberShop.Domain.Entities;
using System.Threading.Tasks;

namespace BarberShop.Domain.Interfaces
{
    public interface IBarbeiroRepository : IRepository<Barbeiro>
    {
        Task<Barbeiro> GetByEmailOrPhoneAsync(string email, string telefone);
    }
}
