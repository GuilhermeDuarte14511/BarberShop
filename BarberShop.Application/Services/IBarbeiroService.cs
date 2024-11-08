using BarberShop.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BarberShop.Application.Services
{
    public interface IBarbeiroService
    {
        Task<IEnumerable<Barbeiro>> ObterTodosBarbeirosAsync();
        Task<Barbeiro> ObterBarbeiroPorIdAsync(int id);
        Task<Barbeiro> VerificarExistenciaPorEmailOuTelefoneAsync(string email, string telefone);
        Task<IEnumerable<Barbeiro>> ObterBarbeirosPorBarbeariaIdAsync(int barbeariaId); // Novo método com barbeariaId


        Task<bool> VerificarDisponibilidadeHorarioAsync(int barbeiroId, DateTime dataHora, int duracao);

    }
}
