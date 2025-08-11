using BarberShop.Application.Interfaces;
using BarberShop.Application.Jobs;
using BarberShop.Application.Services;
using BarberShop.Application.Settings;
using BarberShop.Domain.Entities;
using BarberShop.Domain.Interfaces;
using BarberShop.Infrastructure.Data;
using BarberShop.Infrastructure.Repositories;
using BarberShop.Middlewares;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Stripe;

// Configuração inicial do builder
var builder = WebApplication.CreateBuilder(args);

// Configuração do banco de dados (PostgreSQL local)
builder.Services.AddDbContext<BarbeariaContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default"));
    options.UseSnakeCaseNamingConvention();
});
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// Carregar secrets somente em Development (opcional)
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

// Configuração do Stripe
builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));
StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

// Registrar serviços e repositórios
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<ILogService, LogService>();
// Registrar o PagamentoRepository
builder.Services.AddScoped<IPagamentoRepository, PagamentoRepository>();

// Registrar o serviço e repositório de planos de assinatura
builder.Services.AddScoped<IPlanoAssinaturaService, PlanoAssinaturaService>();
builder.Services.AddScoped<IPlanoAssinaturaRepository, PlanoAssinaturaRepository>();

string sendGridApiKey = builder.Environment.IsDevelopment()
    ? builder.Configuration["SendGridApiKey"]
    : Environment.GetEnvironmentVariable("SendGridApiKey");
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
builder.Services.AddScoped<INotificacaoRepository, NotificacaoRepository>();

builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped<IAgendamentoService, AgendamentoService>();
builder.Services.AddScoped<IBarbeiroService, BarbeiroService>();
builder.Services.AddScoped<IServicoService, ServicoService>();
builder.Services.AddScoped<IAutenticacaoService, AutenticacaoService>();
builder.Services.AddScoped<IFeriadoBarbeariaService, FeriadoBarbeariaService>();
builder.Services.AddScoped<IIndisponibilidadeService, IndisponibilidadeService>();
builder.Services.AddScoped<IAvaliacaoService, AvaliacaoService>();
builder.Services.AddScoped<IBarbeiroServicoService, BarbeiroServicoService>();
builder.Services.AddScoped<IRedefinirSenhaService, RedefinirSenhaService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IPagamentoService, PagamentoService>();
builder.Services.AddScoped<INotificacaoService, NotificacaoService>();
builder.Services.AddScoped<IPushSubscriptionService, PushSubscriptionService>();

// Configuração de Email (SendGrid)
builder.Services.AddScoped<IEmailService, EmailService>(provider =>
{
    var logService = provider.GetRequiredService<ILogService>();
    var configuration = provider.GetRequiredService<IConfiguration>();
    var sendGridApiKey = configuration["SendGridApiKey"];
    return new EmailService(sendGridApiKey, logService, configuration);
});

// Chave pública do Stripe para uso nas Views
builder.Services.AddSingleton(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    return configuration["Stripe:PublishableKey"];
});

builder.Services.AddHttpContextAccessor();

// Configuração de autenticação por cookies
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
    if (builder.Environment.IsDevelopment())
    {
        options.Cookie.SecurePolicy = CookieSecurePolicy.None;
        options.Cookie.SameSite = SameSiteMode.Lax;
    }
    else
    {
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
    }
});

// Configuração de sessões
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Configuração do Quartz.NET
builder.Services.AddQuartz(config =>
{
    var jobKeyAvaliacao = new JobKey("EnviarEmailAvaliacaoJob");
    config.AddJob<EnviarEmailAvaliacaoJob>(opts => opts.WithIdentity(jobKeyAvaliacao));
    config.AddTrigger(opts => opts.ForJob(jobKeyAvaliacao)
        .WithIdentity("EnviarEmailAvaliacaoTrigger")
        .StartNow()
        .WithSimpleSchedule(schedule =>
            schedule.WithIntervalInMinutes(10).RepeatForever()));

    var jobKeyNotificacoes = new JobKey("GerarNotificacoesAgendamentosJob");
    config.AddJob<GerarNotificacoesAgendamentosJob>(opts => opts.WithIdentity(jobKeyNotificacoes));
    config.AddTrigger(opts => opts.ForJob(jobKeyNotificacoes)
        .WithIdentity("GerarNotificacoesTrigger")
        .StartNow()
        .WithSimpleSchedule(schedule =>
            schedule.WithIntervalInMinutes(30).RepeatForever()));
});

builder.Services.AddQuartzHostedService(options =>
{
    options.WaitForJobsToComplete = true;
});

builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Montagem do pipeline
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "BarberShop API V1");
    c.RoutePrefix = "swagger";
});

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseMiddleware<AuthenticationMiddleware>();
app.UseAuthorization();
app.UseStatusCodePagesWithReExecute("/Erro/BarbeariaNaoEncontrada");

// Rotas
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
