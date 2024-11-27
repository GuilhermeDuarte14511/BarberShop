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
            // Verifica se o usuário está autenticado
            if (context.User.Identity.IsAuthenticated)
            {
                var barbeariaUrl = context.Session.GetString("BarbeariaUrl");

                // Verifica se a URL da barbearia está disponível na sessão
                if (!string.IsNullOrEmpty(barbeariaUrl))
                {
                    var currentPath = context.Request.Path.Value.ToLower();

                    // Redireciona para o MenuPrincipal caso esteja na página inicial da barbearia
                    if (currentPath == $"/{barbeariaUrl}".ToLower())
                    {
                        context.Response.Redirect($"/{barbeariaUrl}/Cliente/MenuPrincipal");
                        return;
                    }
                }
            }

            await _next(context);
        }
    }
}
