using BarberShop.Application.Interfaces;
using BarberShop.Application.DTOs;
using BarberShop.Domain.Interfaces;

namespace BarberShop.Application.Services
{
    public class RedefinirSenhaService : IRedefinirSenhaService
    {
        private readonly IClienteRepository _clienteRepository;
        private readonly IAutenticacaoService _autenticacaoService;
        private readonly IBarbeariaRepository _barbeariaRepository;

        public RedefinirSenhaService(IClienteRepository clienteRepository, IAutenticacaoService autenticacaoService, IBarbeariaRepository barbeariaRepository)
        {
            _clienteRepository = clienteRepository;
            _autenticacaoService = autenticacaoService;
            _barbeariaRepository = barbeariaRepository;
        }

        public async Task<bool> ValidarTokenAsync(int clienteId, string token)
        {
            var cliente = await _clienteRepository.GetByIdAsync(clienteId);
            return cliente != null && cliente.TokenRecuperacaoSenha == token && cliente.TokenExpiracao >= DateTime.UtcNow;
        }

        public async Task<bool> RedefinirSenhaAsync(RedefinirSenhaDto redefinirSenhaDto)
        {
            var cliente = await _clienteRepository.GetByIdAsync(redefinirSenhaDto.ClienteId);
            if (cliente == null) return false;

            var novaSenhaHash = _autenticacaoService.HashPassword(redefinirSenhaDto.NovaSenha);

            await _clienteRepository.AtualizarSenhaAsync(
                redefinirSenhaDto.ClienteId,
                novaSenhaHash,
                null, // Remove o token de recuperação
                null  // Remove a data de expiração do token
            );

            return true;
        }

        public async Task<string> ObterUrlRedirecionamentoAsync(int clienteId)
        {
            var cliente = await _clienteRepository.GetByIdAsync(clienteId);
            var barbearia = await _barbeariaRepository.GetByIdAsync(cliente.BarbeariaId);
            return barbearia != null
                ? $"/{barbearia.UrlSlug}/Login"
                : "/Login";
        }
    }
}
