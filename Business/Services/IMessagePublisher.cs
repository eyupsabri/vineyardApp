namespace Business.Services
{
    public interface IMessagePublisher
    {
        Task PublishAsync(string topic, string payload);

    }
}
