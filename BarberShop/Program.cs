using BarberShop.Application.Interfaces;
using BarberShop.Application.Jobs;
using BarberShop.Application.Services;
using BarberShop.Application.Settings;
using BarberShop.Domain.Entities;
using BarberShop.Domain.Interfaces;
using BarberShop.Infrastructure.Data;
using BarberShop.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Stripe;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<BarbeariaContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("BarberShopDb")));

// Carregar secrets somente em Development
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

// Configurar o Stripe
builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));
StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

// Registrar o serviço de pagamento com Stripe
builder.Services.AddScoped<IPaymentService, PaymentService>();

// Registrar o serviço de logging customizado
builder.Services.AddScoped<ILogService, LogService>();

// Registrar o PagamentoRepository
builder.Services.AddScoped<IPagamentoRepository, PagamentoRepository>();

// Registrar o serviço e repositório de planos de assinatura
builder.Services.AddScoped<IPlanoAssinaturaService, PlanoAssinaturaService>();
builder.Services.AddScoped<IPlanoAssinaturaRepository, PlanoAssinaturaRepository>();

// Obter a chave SendGridApiKey dinamicamente com base no ambiente
string sendGridApiKey = builder.Environment.IsDevelopment()
    ? builder.Configuration["SendGridApiKey"]
    : Environment.GetEnvironmentVariable("SendGridApiKey");

// Configurar o serviço de Email com SendGrid usando a chave configurada
builder.Services.AddScoped<IEmailService, EmailService>(provider =>
{
    var logService = provider.GetRequiredService<ILogService>();
    return new EmailService(sendGridApiKey, logService);
});

// Obter a PublishableKey do Stripe e definir para a ViewData na aplicação
builder.Services.AddSingleton(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    return configuration["Stripe:PublishableKey"];
});

builder.Services.AddHttpContextAccessor();


// Registrar repositórios
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


// Registrar serviços da camada de aplicação
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


// Configurar autenticação com cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login/Login";
        options.LogoutPath = "/Login/Logout";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        options.SlidingExpiration = true;
    });

// Configurar sessões
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Tempo de expiração da sessão
    options.Cookie.HttpOnly = true; // Acesso apenas via HTTP
    options.Cookie.IsEssential = true; // Necessário para o funcionamento da aplicação
});

// Lê a expressão cron do appsettings.json
var cronExpression = builder.Configuration["AppSettings:QuartzCron"];

// Verifica se a expressão cron foi configurada, caso contrário, define um padrão (a cada 10 segundos)
if (string.IsNullOrWhiteSpace(cronExpression))
{
    cronExpression = "0/10 * * * * ?"; // Default: a cada 10 segundos
    Console.WriteLine("Expressão cron não configurada. Usando o padrão: 0/10 * * * * ?");
}
else
{
    Console.WriteLine($"Usando a expressão cron configurada: {cronExpression}");
}

// Configura o Quartz.NET
builder.Services.AddQuartz(config =>
{
    // Define o Job
    var jobKey = new JobKey("EnviarEmailAvaliacaoJob");

    config.AddJob<EnviarEmailAvaliacaoJob>(opts =>
    {
        opts.WithIdentity(jobKey);
        Console.WriteLine($"Job 'EnviarEmailAvaliacaoJob' registrado com sucesso.");
    });

    // Define a Trigger
    config.AddTrigger(opts =>
    {
        opts.ForJob(jobKey)
            .WithIdentity("EnviarEmailAvaliacaoTrigger")
            .StartNow() // Inicia imediatamente
            .WithSimpleSchedule(schedule =>
                schedule.WithIntervalInMinutes(10) // Executa a cada 10 minutos
                        .RepeatForever()); // Repete indefinidamente
        Console.WriteLine("Trigger para o Job 'EnviarEmailAvaliacaoJob' configurada para iniciar imediatamente e repetir a cada 10 minutos.");
    });
});

builder.Services.AddQuartzHostedService(options =>
{
    options.WaitForJobsToComplete = true; // Garante que os jobs sejam completados ao encerrar o serviço
});

// Adicionar serviços MVC e configurar o Swagger
builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configurar pipeline de processamento de requisições HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Habilitar o Swagger e a UI do Swagger em todos os ambientes
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "BarberShop API V1");
    c.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Middleware de autenticação, autorização e sessões
app.UseSession(); // Habilita o uso de sessões
app.UseAuthentication();
app.UseAuthorization();

// Configurar redirecionamento para erros de status 404
app.UseStatusCodePagesWithReExecute("/Erro/BarbeariaNaoEncontrada");

// Mapeamento de Rotas para incluir `UrlSlug`
app.MapControllerRoute(
    name: "default",
    pattern: "{barbeariaUrl}/{controller=Login}/{action=Login}/{id?}");

app.MapControllerRoute(
    name: "adminLogin",
    pattern: "{barbeariaUrl}/admin",
    defaults: new { controller = "Login", action = "AdminLogin" }
);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
