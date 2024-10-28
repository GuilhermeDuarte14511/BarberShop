namespace BarberShop.Domain.Interfaces
{
    public interface IMessageQueueService
    {
        void EnviarParaFila(string mensagem);
        void IniciarConsumo();
        void Fechar();
    }
}
