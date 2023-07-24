namespace kafkaconsumer.kafka
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
    }
}