using Avro;
using Avro.Generic;
using Confluent.Kafka;
using kafkaconsumer.Kafka;
using kafkaconsumer.mongo;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;
using static Confluent.Kafka.ConfigPropertyNames;
using MongoDB.Driver;
using kafkaconsumer.Crypt;
using Confluent.SchemaRegistry.Serdes;
using Confluent.SchemaRegistry;
using System.Security.Cryptography.Xml;
using Confluent.Kafka.SyncOverAsync;

namespace kafkaconsumer
{
    public class ConsumerService : IHostedService
    {
        private readonly ClientConfig _clientConfig;        
        private readonly string _topic;
        private CancellationTokenSource _cancellationToken;
        private readonly IDecryptAsymmetric _decryptAsymmetric;
        private readonly IConsumerSettings _consumerSettings;
        private readonly IConsumerClient _consumerClient; 


        public ConsumerService(IConfiguration config, IDecryptAsymmetric decryptAsymmetric, IConsumerClient consumerClient /*, IMongoHelper mongoHelper*/)
        {
            _consumerSettings = config?.GetSection("KafkaSettings")?.Get<ConsumerSettings>();            

            _decryptAsymmetric = decryptAsymmetric;
            _consumerClient = consumerClient;

            _clientConfig = new ConsumerConfig()
            {
                BootstrapServers = _consumerSettings?.BootstrapServers,
                SecurityProtocol = SecurityProtocol.SaslSsl,
                SaslMechanism = SaslMechanism.Plain,
                SaslUsername = _consumerSettings?.SaslUsername,
                SaslPassword = _consumerSettings?.SaslPassword,
                GroupId = _consumerSettings?.GroupId,

                EnableAutoOffsetStore = false,
                EnableAutoCommit = true,
                AutoCommitIntervalMs = 1000,
                
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            _topic = _consumerSettings.Topic;            

            // Decrypt the password
            string mongoPassword = string.Empty;
            var cryptoSettings = _decryptAsymmetric?.GetConfigSettings();
            if ((cryptoSettings != null))
            {               
                byte[] cipherText = Convert.FromBase64String(_consumerSettings?.SaslPassword);
                if (cipherText?.Length > 0)
                {
                    var decryptedPass = _decryptAsymmetric?.DecryptAsymmetricString(cipherText);
                    _clientConfig.SaslPassword = decryptedPass.Replace("\n", string.Empty);
                }                                           
            }                        
        }
        
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            do
            {
                var initResult = _consumerClient.Initialize(_clientConfig, _topic, cancellationToken);
                if (initResult.Exception == null)
                {
                    await Task.CompletedTask;
                    return;
                }
            } while(!cancellationToken.IsCancellationRequested);            
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }        
    }
}
