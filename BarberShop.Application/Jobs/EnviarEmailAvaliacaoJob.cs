using Microsoft.Extensions.Configuration;
using Quartz;
using System;
using System.Linq;
using System.Threading.Tasks;
using BarberShop.Domain.Entities;
using BarberShop.Domain.Interfaces;
using BarberShop.Application.Services;

namespace BarberShop.Application.Jobs
{
    public class EnviarEmailAvaliacaoJob : IJob
    {
        private readonly IAgendamentoService _agendamentoService;
        private readonly IEmailService _emailService;
        private readonly IAgendamentoRepository _agendamentoRepository;
        private readonly string _baseUrl;
        private readonly ILogService _logService;  // Adicionando o log service

        public EnviarEmailAvaliacaoJob(
            IAgendamentoService agendamentoService,
            IEmailService emailService,
            IAgendamentoRepository agendamentoRepository,
            IConfiguration configuration,
            ILogService logService) // Injeção de ILogService
        {
            _agendamentoService = agendamentoService;
            _emailService = emailService;
            _agendamentoRepository = agendamentoRepository;
            _logService = logService; // Inicializando o log service
            _baseUrl = configuration["AppSettings:BaseUrl"];
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                // Iniciando o log do job
                await _logService.SaveLogAsync("Information", "EnviarEmailAvaliacaoJob", "Iniciando o envio de e-mails de avaliação", "", null);

                var agendamentosConcluidos = await _agendamentoService.ObterAgendamentosConcluidosAsync();

                foreach (var agendamento in agendamentosConcluidos)
                {
                    var clienteEmail = agendamento.Cliente.Email;

                    // Constrói a mensagem e a URL de avaliação
                    var mensagem = $"Olá {agendamento.Cliente.Nome}, avalie seu atendimento conosco!";
                    var avaliacaoUrl = $"{_baseUrl}/Avaliacao/Index?agendamentoId={agendamento.AgendamentoId}";

                    // Envia o e-mail
                    await _emailService.EnviaEmailAvaliacao(
                        agendamentoId: agendamento.AgendamentoId,
                        destinatarioEmail: clienteEmail,
                        destinatarioNome: agendamento.Cliente.Nome,
                        nomeBarbearia: agendamento.Barbearia.Nome,
                        urlBase: _baseUrl
                    );

                    // Marca o e-mail como enviado
                    agendamento.EmailEnviado = true;
                    await _agendamentoRepository.UpdateAsync(agendamento);

                    // Log do envio do e-mail
                    await _logService.SaveLogAsync("Information", "EnviarEmailAvaliacaoJob", $"E-mail enviado para: {clienteEmail}", "", agendamento.AgendamentoId.ToString());
                }

                // Salva as alterações no banco
                await _agendamentoRepository.SaveChangesAsync();

                // Finalizando o log do job
                await _logService.SaveLogAsync("Information", "EnviarEmailAvaliacaoJob", "Job de envio de e-mails de avaliação finalizado.", "", null);
            }
            catch (Exception ex)
            {
                // Log de erro
                await _logService.SaveLogAsync("Error", "EnviarEmailAvaliacaoJob", "Erro ao enviar os e-mails de avaliação", ex.Message, null);
                throw;
            }
        }
    }
}
