using BarberShop.Application.Interfaces;
using BarberShop.Application.Services;
using Quartz;

public class GerarNotificacoesAgendamentosJob : IJob
{
    private readonly INotificacaoService _notificacaoService;

    public GerarNotificacoesAgendamentosJob(INotificacaoService notificacaoService)
    {
        _notificacaoService = notificacaoService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _notificacaoService.GerarNotificacoesDeAgendamentosProximos();
        await Task.CompletedTask;
    }
}

