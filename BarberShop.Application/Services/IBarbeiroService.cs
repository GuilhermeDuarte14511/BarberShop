using BarberShop.Application.DTOs;
using BarberShop.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BarberShop.Application.Services
{
    public interface IBarbeiroService
    {
        Task<IEnumerable<Barbeiro>> ObterTodosBarbeirosAsync();
        Task<Barbeiro> ObterBarbeiroPorIdAsync(int id);
        Task<Barbeiro> VerificarExistenciaPorEmailOuTelefoneAsync(string email, string telefone);
        Task<IEnumerable<Barbeiro>> ObterBarbeirosPorBarbeariaIdAsync(int barbeariaId);
        Task<bool> VerificarDisponibilidadeHorarioAsync(int barbeiroId, DateTime dataHora, int duracao);
        Task<IEnumerable<Barbeiro>> ObterBarbeirosPorServicosAsync(int barbeariaId, List<int> servicoIds);
        Task<Barbeiro> CriarBarbeiroAsync(Barbeiro barbeiro);
        Task<bool> DeletarBarbeiroAsync(int id);
        Task<bool> AtualizarBarbeiroEUsuarioAsync(AtualizarBarbeiroUsuarioDto dto);


    }
}
