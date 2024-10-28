namespace BarberShop.Application.Interfaces
{
    public interface IRabbitMQService
    {
        void EnviarParaFila(string mensagem);
        void IniciarConsumo();
        void Fechar();
    }
}
