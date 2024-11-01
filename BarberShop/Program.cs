using BarberShop.Application.Services;
using BarberShop.Application.Settings;
using BarberShop.Domain.Entities;
using BarberShop.Domain.Interfaces;
using BarberShop.Infrastructure.Data;
using BarberShop.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Stripe;

var builder = WebApplication.CreateBuilder(args);

// Configurar a string de conex�o usando o appsettings.json
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

// Registrar o servi�o de pagamento com Stripe
builder.Services.AddScoped<IPaymentService, PaymentService>();

// Registrar o servi�o de logging customizado
builder.Services.AddScoped<ILogService, LogService>();

// Obter a chave SendGridApiKey dinamicamente com base no ambiente
string sendGridApiKey = builder.Environment.IsDevelopment()
    ? builder.Configuration["SendGridApiKey"]  // Obt�m dos secrets em Development
    : Environment.GetEnvironmentVariable("SendGridApiKey"); // Obt�m da vari�vel de ambiente na Azure

// Configurar o servi�o de Email com SendGrid usando a chave configurada
builder.Services.AddScoped<IEmailService, EmailService>(provider =>
{
    var logService = provider.GetRequiredService<ILogService>(); // Obt�m a inst�ncia de ILogService
    return new EmailService(sendGridApiKey, logService); // Passa sendGridApiKey e logService para o construtor
});
// Obter a PublishableKey do Stripe e definir para a ViewData na aplica��o
builder.Services.AddSingleton(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    return configuration["Stripe:PublishableKey"];
});

// Servi�o RabbitMQ (Comentado porque voc� n�o ir� usar agora)
/*
builder.Services.AddSingleton<IRabbitMQService, RabbitMQService>(provider =>
    new RabbitMQService(builder.Configuration["SendGridApiKey"], provider));
*/

// Registrar reposit�rios
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<IBarbeiroRepository, BarbeiroRepository>();
builder.Services.AddScoped<IServicoRepository, ServicoRepository>();
builder.Services.AddScoped<IAgendamentoRepository, AgendamentoRepository>();
builder.Services.AddScoped<IRepository<AgendamentoServico>, AgendamentoServicoRepository>();

// Registrar servi�os da camada de aplica��o
builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped<IAgendamentoService, AgendamentoService>();
builder.Services.AddScoped<IBarbeiroService, BarbeiroService>();

// Registrar o AutenticacaoService
builder.Services.AddScoped<AutenticacaoService>();

// Configurar autentica��o com cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login/Login";   // Define o caminho da p�gina de login
        options.LogoutPath = "/Login/Logout"; // Define o caminho da p�gina de logout
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60); // Tempo de expira��o da sess�o (opcional)
        options.SlidingExpiration = true; // Expira��o deslizante (opcional)
    });

// Adicionar servi�os MVC
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configurar pipeline de processamento de requisi��es HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Middleware de autentica��o e autoriza��o
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Login}/{id?}");

// Inicializa o consumidor RabbitMQ ao iniciar o aplicativo (Comentado)
/*
var rabbitMQService = app.Services.GetRequiredService<IRabbitMQService>();
Task.Run(() => rabbitMQService.IniciarConsumo());
*/

app.Run();
