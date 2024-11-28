using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace BarberShop.Middlewares
{
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthenticationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.User.Identity.IsAuthenticated)
            {
                // Recuperar BarbeariaId e BarbeariaUrl da sessão
                var barbeariaId = context.Session.GetInt32("BarbeariaId");
                var barbeariaUrl = context.Session.GetString("BarbeariaUrl");

                // Configurar BarbeariaId e BarbeariaUrl caso não estejam na sessão
                if (!barbeariaId.HasValue || string.IsNullOrEmpty(barbeariaUrl))
                {
                    barbeariaId = int.TryParse(context.User.FindFirst("BarbeariaId")?.Value, out var id) ? id : (int?)null;
                    barbeariaUrl = context.User.FindFirst("urlSlug")?.Value;

                    if (barbeariaId.HasValue && !string.IsNullOrEmpty(barbeariaUrl))
                    {
                        context.Session.SetInt32("BarbeariaId", barbeariaId.Value);
                        context.Session.SetString("BarbeariaUrl", barbeariaUrl);
                    }
                }

                // Realiza redirecionamentos com base no contexto da barbearia
                if (!string.IsNullOrEmpty(barbeariaUrl))
                {
                    var currentPath = context.Request.Path.Value.ToLower();

                    if (currentPath == $"/{barbeariaUrl}".ToLower())
                    {
                        context.Response.Redirect($"/{barbeariaUrl}/Cliente/MenuPrincipal");
                        return;
                    }

                    if (currentPath == $"/{barbeariaUrl}/admin".ToLower() &&
                        (context.User.IsInRole("Admin") || context.User.IsInRole("Barbeiro")))
                    {
                        context.Response.Redirect($"/{barbeariaUrl}/Admin/Index");
                        return;
                    }
                }
            }

            // Continua para o próximo middleware ou controlador
            await _next(context);
        }
    }
}
