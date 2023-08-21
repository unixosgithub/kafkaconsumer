using Confluent.Kafka;

namespace kafkaconsumer
{
    public interface IConsumerClient
    {
        Task Initialize(ClientConfig config, string topic, CancellationToken cancellationToken);
        Task ProcessMessages(ConsumerBuilder<Ignore, string> builder, string topic, CancellationToken cancellationToken);
    }
}