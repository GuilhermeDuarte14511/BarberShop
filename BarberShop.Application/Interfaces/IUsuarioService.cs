using BarberShop.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BarberShop.Application.Services
{
    public interface IUsuarioService
    {
        Task<IEnumerable<Usuario>> ListarUsuariosPorBarbeariaAsync(int barbeariaId);
        Task<Usuario> CriarUsuarioAsync(Usuario usuario);
        Task<Usuario> AtualizarUsuarioAsync(Usuario usuarioAtualizado);
        Task<bool> DeletarUsuarioAsync(int usuarioId);
        Task<Usuario> ObterUsuarioPorIdAsync(int usuarioId);
        Task<List<Usuario>> ObterUsuariosPorEmailOuTelefoneAsync(string email, string telefone);


    }
}
