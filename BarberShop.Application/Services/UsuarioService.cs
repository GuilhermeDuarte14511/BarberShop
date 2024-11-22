using BarberShop.Domain.Entities;
using BarberShop.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BarberShop.Application.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _usuarioRepository;

        public UsuarioService(IUsuarioRepository usuarioRepository)
        {
            _usuarioRepository = usuarioRepository;
        }

        public async Task<IEnumerable<Usuario>> ListarUsuariosPorBarbeariaAsync(int barbeariaId)
        {
            var usuarios = (await _usuarioRepository.GetAllAsync())
                .Where(u => u.BarbeariaId == barbeariaId)
                .ToList();
            return usuarios;
        }

        public async Task<Usuario> CriarUsuarioAsync(Usuario usuario)
        {
            if (string.IsNullOrEmpty(usuario.Nome) || string.IsNullOrEmpty(usuario.Email))
                throw new ArgumentException("Dados inválidos para criação do usuário.");

            usuario.DataCriacao = DateTime.Now;
            var novoUsuario = await _usuarioRepository.AddAsync(usuario);
            await _usuarioRepository.SaveChangesAsync();
            return novoUsuario;
        }

        public async Task<Usuario> AtualizarUsuarioAsync(Usuario usuarioAtualizado)
        {
            if (usuarioAtualizado == null || usuarioAtualizado.UsuarioId <= 0)
                throw new ArgumentException("Dados inválidos para atualização do usuário.");

            var usuarioExistente = await _usuarioRepository.GetByIdAsync(usuarioAtualizado.UsuarioId);
            if (usuarioExistente == null)
                throw new KeyNotFoundException("Usuário não encontrado.");

            usuarioExistente.Nome = usuarioAtualizado.Nome;
            usuarioExistente.Email = usuarioAtualizado.Email;
            usuarioExistente.Telefone = usuarioAtualizado.Telefone;
            usuarioExistente.Role = usuarioAtualizado.Role;
            usuarioExistente.Status = usuarioAtualizado.Status;

            await _usuarioRepository.UpdateAsync(usuarioExistente);
            await _usuarioRepository.SaveChangesAsync();
            return usuarioExistente;
        }

        public async Task<bool> DeletarUsuarioAsync(int usuarioId)
        {
            var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);
            if (usuario == null)
                throw new KeyNotFoundException("Usuário não encontrado.");

            await _usuarioRepository.DeleteAsync(usuario);
            await _usuarioRepository.SaveChangesAsync();
            return true;
        }

        public async Task<Usuario> ObterUsuarioPorIdAsync(int usuarioId)
        {
            var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);
            if (usuario == null)
                throw new KeyNotFoundException("Usuário não encontrado.");
            return usuario;
        }

        public async Task<List<Usuario>> ObterUsuariosPorEmailOuTelefoneAsync(string email, string telefone)
        {
            return await _usuarioRepository.ObterPorEmailOuTelefoneAsync(email, telefone);
        }
    }
}
