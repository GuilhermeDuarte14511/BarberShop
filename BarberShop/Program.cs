using BarberShop.Application.Interfaces;
using BarberShop.Application.Jobs;
using BarberShop.Application.Services;
using BarberShop.Application.Settings;
using BarberShop.Domain.Entities;
using BarberShop.Domain.Interfaces;
using BarberShop.Infrastructure.Data;
using BarberShop.Infrastructure.Repositories;
using BarberShop.Middlewares; // Adicione o namespace para acessar o middleware
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Stripe;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<BarbeariaContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("EspacoBarberShopOficial")));

// Carregar secrets somente em Development
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

// Configurar o Stripe
builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));
StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

// Registrar o servi�o de pagamento com Stripe
builder.Services.AddScoped<IPaymentService, PaymentService>();

// Registrar o servi�o de logging customizado
builder.Services.AddScoped<ILogService, LogService>();

// Registrar o PagamentoRepository
builder.Services.AddScoped<IPagamentoRepository, PagamentoRepository>();

// Registrar o servi�o e reposit�rio de planos de assinatura
builder.Services.AddScoped<IPlanoAssinaturaService, PlanoAssinaturaService>();
builder.Services.AddScoped<IPlanoAssinaturaRepository, PlanoAssinaturaRepository>();

// Obter a chave SendGridApiKey dinamicamente com base no ambiente
string sendGridApiKey = builder.Environment.IsDevelopment()
    ? builder.Configuration["SendGridApiKey"]
    : Environment.GetEnvironmentVariable("SendGridApiKey");

// Configurar o servi�o de Email com SendGrid usando a chave configurada
builder.Services.AddScoped<IEmailService, EmailService>(provider =>
{
    var logService = provider.GetRequiredService<ILogService>();
    var configuration = provider.GetRequiredService<IConfiguration>(); // Obt�m o IConfiguration
    var sendGridApiKey = configuration["SendGridApiKey"]; // Obt�m a chave da configura��o

    return new EmailService(sendGridApiKey, logService, configuration);
});

// Obter a PublishableKey do Stripe e definir para a ViewData na aplica��o
builder.Services.AddSingleton(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    return configuration["Stripe:PublishableKey"];
});

builder.Services.AddHttpContextAccessor();

// Registrar reposit�rios
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<IBarbeiroRepository, BarbeiroRepository>();
builder.Services.AddScoped<IServicoRepository, ServicoRepository>();
builder.Services.AddScoped<IAgendamentoRepository, AgendamentoRepository>();
builder.Services.AddScoped<IRepository<AgendamentoServico>, AgendamentoServicoRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();
builder.Services.AddScoped<IRelatorioPersonalizadoRepository, RelatorioPersonalizadoRepository>();
builder.Services.AddScoped<IPagamentoRepository, PagamentoRepository>();
builder.Services.AddScoped<IAvaliacaoRepository, AvaliacaoRepository>();
builder.Services.AddScoped<IBarbeariaRepository, BarbeariaRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IFeriadoNacionalRepository, FeriadoNacionalRepository>();
builder.Services.AddScoped<IFeriadoBarbeariaRepository, FeriadoBarbeariaRepository>();
builder.Services.AddScoped<IIndisponibilidadeRepository, IndisponibilidadeRepository>();
builder.Services.AddScoped<IBarbeiroServicoRepository, BarbeiroServicoRepository>();
builder.Services.AddScoped<IPagamentoRepository, PagamentoRepository>();
builder.Services.AddScoped<INotificacaoRepository, NotificacaoRepository>();
builder.Services.AddScoped<IOnboardingProgressoRepository, OnboardingProgressoRepository>(); // Reposit�rio do Onboarding

// Registrar servi�os da camada de aplica��o
builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped<IAgendamentoService, AgendamentoService>();
builder.Services.AddScoped<IBarbeiroService, BarbeiroService>();
builder.Services.AddScoped<IServicoService, ServicoService>();
builder.Services.AddScoped<IAutenticacaoService, AutenticacaoService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IFeriadoBarbeariaService, FeriadoBarbeariaService>();
builder.Services.AddScoped<IIndisponibilidadeService, IndisponibilidadeService>();
builder.Services.AddScoped<IAvaliacaoService, AvaliacaoService>();
builder.Services.AddScoped<IBarbeiroServicoService, BarbeiroServicoService>();
builder.Services.AddScoped<IRedefinirSenhaService, RedefinirSenhaService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IPagamentoService, PagamentoService>();
builder.Services.AddScoped<INotificacaoService, NotificacaoService>();
builder.Services.AddScoped<IPushSubscriptionService, PushSubscriptionService>();
builder.Services.AddScoped<IOnboardingService, OnboardingService>(); // Servi�o do Onboarding

// Configurar autentica��o com cookies (apenas uma vez)
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
}).AddCookie(options =>
{
    options.LoginPath = "/Login/Login";
    options.LogoutPath = "/Login/Logout";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

// Configurar sess�es
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Configura o Quartz.NET
builder.Services.AddQuartz(config =>
{
    var jobKeyAvaliacao = new JobKey("EnviarEmailAvaliacaoJob");

    config.AddJob<EnviarEmailAvaliacaoJob>(opts =>
    {
        opts.WithIdentity(jobKeyAvaliacao);
    });

    config.AddTrigger(opts =>
    {
        opts.ForJob(jobKeyAvaliacao)
            .WithIdentity("EnviarEmailAvaliacaoTrigger")
            .StartNow()
            .WithSimpleSchedule(schedule =>
                schedule.WithIntervalInMinutes(10)
                        .RepeatForever());
    });

    // Configura��o do Job para Gerar Notifica��es de Agendamentos Pr�ximos
    var jobKeyNotificacoes = new JobKey("GerarNotificacoesAgendamentosJob");

    config.AddJob<GerarNotificacoesAgendamentosJob>(opts =>
    {
        opts.WithIdentity(jobKeyNotificacoes);
    });

    config.AddTrigger(opts =>
    {
        opts.ForJob(jobKeyNotificacoes)
            .WithIdentity("GerarNotificacoesTrigger")
            .StartNow()
            .WithSimpleSchedule(schedule =>
                schedule.WithIntervalInMinutes(30)
                        .RepeatForever());
    });
});

// Adiciona o Quartz Hosted Service
builder.Services.AddQuartzHostedService(options =>
{
    options.WaitForJobsToComplete = true;
});

builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

app.UseExceptionHandler("/Home/Error");
app.UseHsts();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "BarberShop API V1");
    c.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseMiddleware<AuthenticationMiddleware>();
app.UseAuthorization(); 

app.UseStatusCodePagesWithReExecute("/Erro/BarbeariaNaoEncontrada");

// Configura��o de rotas
app.MapControllerRoute(
    name: "default",
    pattern: "{barbeariaUrl}/{controller=Login}/{action=Login}/{id?}");

app.MapControllerRoute(
    name: "adminLogin",
    pattern: "{barbeariaUrl}/admin",
    defaults: new { controller = "Login", action = "AdminLogin" });

app.MapControllerRoute(
    name: "home",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
