namespace BarberShop.Domain.Entities
{
    public class PushSubscription
    {
        public int Id { get; set; }
        public int? UsuarioId { get; set; } // ID do usuário associado (pode ser nulo)
        public string Endpoint { get; set; } // Endpoint para o serviço de notificações
        public string P256dh { get; set; } // Chave pública gerada pelo navegador
        public string Auth { get; set; } // Chave de autenticação gerada pelo navegador
        public DateTime DataCadastro { get; set; } // Data de registro
    }
}
