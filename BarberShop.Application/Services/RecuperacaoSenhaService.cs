using BarberShop.Domain.Entities;
using BarberShop.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberShop.Application.Services
{
    public class RecuperacaoSenhaService : IRecuperacaoSenhaService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IEmailService _emailService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAutenticacaoService _autenticacaoService;
        private readonly IClienteRepository _clienteRepository;

        public RecuperacaoSenhaService(
            IUsuarioRepository usuarioRepository,
            IEmailService emailService,
            IHttpContextAccessor httpContextAccessor,
            IAutenticacaoService autenticacaoService,
            IClienteRepository clienteRepository)
        {
            _usuarioRepository = usuarioRepository;
            _emailService = emailService;
            _httpContextAccessor = httpContextAccessor;
            _autenticacaoService = autenticacaoService;
            _clienteRepository = clienteRepository;
        }


        public async Task EnviarEmailRecuperacaoSenhaAsync(string email)
        {

            var cliente = await _clienteRepository.GetByEmailAsync(email);
            if (cliente == null) throw new Exception("Usuário não encontrado.");

            var token = Guid.NewGuid().ToString();
            cliente.TokenRecuperacaoSenha = token;
            cliente.TokenExpiracao = DateTime.UtcNow.AddHours(1);

            await _clienteRepository.UpdateAsync(cliente);

            // Obtenha a URL base dinamicamente
            var request = _httpContextAccessor.HttpContext.Request;
            var urlBase = $"{request.Scheme}://{request.Host}";

            var link = $"{urlBase}/redefinir-senha?clienteId={cliente.ClienteId}&token={token}";

            await _emailService.EnviarEmailRecuperacaoSenhaAsync(cliente.Email, cliente.Nome, link);
        }

        public async Task<bool> VerificarTokenAsync(int clienteId, string token)
        {
            var cliente = await _clienteRepository.GetByIdAsync(clienteId);
            return cliente != null && cliente.TokenRecuperacaoSenha == token && cliente.TokenExpiracao > DateTime.UtcNow;
        }

        public async Task<bool> RedefinirSenhaAsync(int clienteId, string novaSenha)
        {
            var cliente = await _clienteRepository.GetByIdAsync(clienteId);
            if (cliente == null || cliente.TokenExpiracao < DateTime.UtcNow)
                return false;

            // Hash da nova senha
            cliente.Senha = _autenticacaoService.HashPassword(novaSenha);
            cliente.TokenRecuperacaoSenha = null;
            cliente.TokenExpiracao = null;

            await _clienteRepository.UpdateAsync(cliente);
            return true;
        }

    }
}
