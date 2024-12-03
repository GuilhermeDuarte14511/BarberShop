using BarberShop.Application.DTOs;
using BarberShop.Application.Interfaces;
using BarberShop.Domain.Entities;
using BarberShop.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BarberShop.Application.Services
{
    public class NotificacaoService : INotificacaoService
    {
        private readonly INotificacaoRepository _notificacaoRepository;

        public NotificacaoService(INotificacaoRepository notificacaoRepository)
        {
            _notificacaoRepository = notificacaoRepository;
        }

        public void NotificarAdmins(int barbeariaId, Agendamento agendamento)
        {
           var barbearia = _notificacaoRepository.ObterBarbeariaPorId(barbeariaId);

            var admins = _notificacaoRepository.ObterAdminsPorBarbearia(barbeariaId);

            foreach (var admin in admins)
            {
                // Verifica se já existe uma notificação para este admin e agendamento
                var notificacaoJaEnviada = _notificacaoRepository
                .ObterNotificacoes(admin.UsuarioId, agendamento.AgendamentoId)
                .Any(n => n.UsuarioId == admin.UsuarioId && n.AgendamentoId == agendamento.AgendamentoId && n.BarbeariaId == barbeariaId);

                if (!notificacaoJaEnviada)
                {
                    Console.WriteLine($"[INFO] Criando nova notificação para Admin ID: {admin.UsuarioId}, Agendamento ID: {agendamento.AgendamentoId}");

                    var notificacao = new Notificacao
                    {
                        UsuarioId = admin.UsuarioId,
                        BarbeariaId = barbeariaId,
                        AgendamentoId = agendamento.AgendamentoId,
                        Mensagem = $"Novo agendamento em {agendamento.DataHora:dd/MM/yyyy HH:mm} com o cliente {agendamento.Cliente.Nome} e barbeiro {agendamento.Barbeiro.Nome}.",
                        Link = $"/{barbearia.UrlSlug}/Agendamento/Index?agendamentoId={agendamento.AgendamentoId}",
                        Lida = false,
                        DataHora = DateTime.Now
                    };
                    _notificacaoRepository.Criar(notificacao);
                }
                else
                {
                    Console.WriteLine($"[INFO] Notificação já existente para Admin ID: {admin.UsuarioId}, Agendamento ID: {agendamento.AgendamentoId}");
                }
            }
        }


        public void NotificarBarbeiro(Agendamento agendamento)
        {
            var barbeiros = _notificacaoRepository.ObterBarbeirosPorBarbearia(agendamento.BarbeariaId);
            var barbeiro = barbeiros.FirstOrDefault(b => b.BarbeiroId == agendamento.BarbeiroId);
            var barbearia = _notificacaoRepository.ObterBarbeariaPorId(agendamento.BarbeariaId);

            if (barbeiro != null)
            {
                Console.WriteLine($"[INFO] Notificando barbeiro ID: {barbeiro.UsuarioId} para o agendamento ID: {agendamento.AgendamentoId}");

                // Define o horário de notificação (1 hora antes do agendamento)
                var referencia = agendamento.DataHora.AddHours(-1);

                // Verifica se já existe uma notificação para este barbeiro no horário específico
                var notificacaoJaEnviada = _notificacaoRepository
                    .ObterNotificacoes(barbeiro.UsuarioId, agendamento.AgendamentoId)
                    .Any(n => n.DataHora == referencia);

                if (!notificacaoJaEnviada)
                {
                    Console.WriteLine($"[INFO] Criando nova notificação para Barbeiro ID: {barbeiro.UsuarioId}, Agendamento ID: {agendamento.AgendamentoId}, Horário de referência: {referencia}");

                    var notificacao = new Notificacao
                    {
                        UsuarioId = barbeiro.UsuarioId,
                        BarbeariaId = agendamento.BarbeariaId,
                        AgendamentoId = agendamento.AgendamentoId,
                        Mensagem = $"Você tem um agendamento às {agendamento.DataHora:HH:mm} com o cliente {agendamento.Cliente.Nome}.",
                        Link = $"/{barbearia.UrlSlug}/Barbeiro/MeusAgendamentos?agendamentoId={agendamento.AgendamentoId}",
                        Lida = false,
                        DataHora = referencia // Apenas uma notificação 1 hora antes
                    };
                    _notificacaoRepository.Criar(notificacao);
                }
                else
                {
                    Console.WriteLine($"[INFO] Notificação já existente para Barbeiro ID: {barbeiro.UsuarioId}, Agendamento ID: {agendamento.AgendamentoId}, Horário de referência: {referencia}");
                }
            }
        }




        public void GerarNotificacoesDeAgendamentosProximos()
        {
            var hoje = DateTime.Now.Date;
            Console.WriteLine($"[INFO] Iniciando geração de notificações para o dia {hoje:dd/MM/yyyy}");

            var agendamentosHoje = _notificacaoRepository.ObterAgendamentosDoDia(hoje);

            Console.WriteLine($"[INFO] Total de agendamentos encontrados: {agendamentosHoje.Count()}");

            foreach (var agendamento in agendamentosHoje)
            {
                Console.WriteLine($"[INFO] Processando agendamento ID: {agendamento.AgendamentoId}, Horário: {agendamento.DataHora}, Cliente: {agendamento.ClienteId}, Barbeiro: {agendamento.BarbeiroId}");

                // Notifica os admins
                NotificarAdmins(agendamento.BarbeariaId, agendamento);

                // Notifica o barbeiro
                NotificarBarbeiro(agendamento);
            }

            Console.WriteLine("[INFO] Finalizada a geração de notificações.");
        }

        public IEnumerable<NotificacaoDTO> ObterPorUsuario(int usuarioId)
        {
            var notificacoes = _notificacaoRepository.ObterPorUsuario(usuarioId);

            return notificacoes.Select(n => new NotificacaoDTO
            {
                NotificacaoId = n.NotificacaoId,
                UsuarioId = n.UsuarioId,
                BarbeariaId = n.BarbeariaId,
                AgendamentoId = n.AgendamentoId,
                Mensagem = n.Mensagem,
                Link = n.Link,
                Lida = n.Lida,
                DataHora = n.DataHora
            });
        }

        public IEnumerable<object> ObterNotificacoesAgrupadasPorDia(int usuarioId)
        {
            var notificacoes = _notificacaoRepository.ObterPorUsuario(usuarioId);

            var agrupadas = notificacoes
                .GroupBy(n => n.DataHora.Date)
                .Select(g => new
                {
                    Data = g.Key,
                    NaoLidas = g.Where(n => !n.Lida).Select(n => new NotificacaoDTO
                    {
                        NotificacaoId = n.NotificacaoId,
                        UsuarioId = n.UsuarioId,
                        BarbeariaId = n.BarbeariaId,
                        AgendamentoId = n.AgendamentoId,
                        Mensagem = n.Mensagem,
                        Link = n.Link,
                        Lida = n.Lida,
                        DataHora = n.DataHora
                    }).ToList(),
                    Lidas = g.Where(n => n.Lida).Select(n => new NotificacaoDTO
                    {
                        NotificacaoId = n.NotificacaoId,
                        UsuarioId = n.UsuarioId,
                        BarbeariaId = n.BarbeariaId,
                        AgendamentoId = n.AgendamentoId,
                        Mensagem = n.Mensagem,
                        Link = n.Link,
                        Lida = n.Lida,
                        DataHora = n.DataHora
                    }).ToList()
                });

            return agrupadas;
        }

        public void MarcarTodasComoLidas(int usuarioId)
        {
            var notificacoes = _notificacaoRepository.ObterPorUsuario(usuarioId);
            foreach (var notificacao in notificacoes)
            {
                notificacao.Lida = true;
            }
            _notificacaoRepository.SalvarAlteracoes();
        }

        public void CriarNotificacao(NotificacaoDTO notificacaoDTO)
        {
            var notificacao = new Notificacao
            {
                UsuarioId = notificacaoDTO.UsuarioId.Value,
                BarbeariaId = notificacaoDTO.BarbeariaId,
                AgendamentoId = notificacaoDTO.AgendamentoId,
                Mensagem = notificacaoDTO.Mensagem,
                Link = notificacaoDTO.Link,
                Lida = false,
                DataHora = DateTime.Now
            };

            _notificacaoRepository.Criar(notificacao);
        }

        public void NotificarAvaliacaoRecebida(Avaliacao avaliacao)
        {
            var agendamento = _notificacaoRepository.ObterAgendamentoPorId(avaliacao.AgendamentoId);
            var barbearia = _notificacaoRepository.ObterBarbeariaPorId(agendamento.BarbeariaId);

            // Notificar todos os admins da barbearia
            var admins = _notificacaoRepository.ObterAdminsPorBarbearia(agendamento.BarbeariaId);
            foreach (var admin in admins)
            {
                var notificacao = new Notificacao
                {
                    UsuarioId = admin.UsuarioId,
                    BarbeariaId = agendamento.BarbeariaId,
                    AgendamentoId = agendamento.AgendamentoId,
                    Mensagem = $"Uma nova avaliação foi recebida para o agendamento em {agendamento.DataHora:dd/MM/yyyy HH:mm}.",
                    Link = $"/{barbearia.UrlSlug}/Avaliacao/Detalhes?avaliacaoId={avaliacao.AvaliacaoId}",
                    Lida = false,
                    DataHora = DateTime.Now
                };
                _notificacaoRepository.Criar(notificacao);
            }

            // Notificar o barbeiro responsável
            var barbeiro = _notificacaoRepository.ObterBarbeiroPorId(agendamento.BarbeiroId);
            if (barbeiro != null)
            {
                var notificacao = new Notificacao
                {
                    UsuarioId = barbeiro.UsuarioId,
                    BarbeariaId = agendamento.BarbeariaId,
                    AgendamentoId = agendamento.AgendamentoId,
                    Mensagem = $"Você recebeu uma nova avaliação para o agendamento em {agendamento.DataHora:dd/MM/yyyy HH:mm}.",
                    Link = $"/{barbearia.UrlSlug}/Barbeiro/Avaliacoes?avaliacaoId={avaliacao.AvaliacaoId}",
                    Lida = false,
                    DataHora = DateTime.Now
                };
                _notificacaoRepository.Criar(notificacao);
            }
        }



    }
}
