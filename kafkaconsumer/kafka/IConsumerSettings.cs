using Confluent.Kafka;

namespace kafkaconsumer.Kafka
{
    public interface IConsumerSettings
    {
        string BootstrapServers { get; set; }
        string SecurityProtocol { get; set; }
        string SaslMechanisms { get; set; }
        string SaslUsername { get; set; }
        string SaslPassword { get; set; }
        string Topic { get; set; }
        string GroupId { get; set; }

        bool EnableAutoOffsetStore { get; set; }
        bool EnableAutoCommit { get; set; }
        int AutoCommitIntervalMs { get; set; }

        AutoOffsetReset AutoOffsetReset { get; set; }
    }
}