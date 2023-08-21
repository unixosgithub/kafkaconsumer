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
        private readonly ConsumerBuilder<string, string> _consumerBuilder;
        private readonly Confluent.Kafka.IConsumer<string, string> _consumer;
        private readonly string _topic;
        private CancellationTokenSource _cancellationToken;
        private readonly IDecryptAsymmetric _decryptAsymmetric;
        private readonly IConsumerSettings _consumerSettings;
        private readonly IMongoHelper _mongoHelper;       


        public ConsumerService(IConfiguration config, IDecryptAsymmetric decryptAsymmetric, IMongoHelper mongoHelper)
        {
            _consumerSettings = config?.GetSection("KafkaSettings")?.Get<ConsumerSettings>();            

            _decryptAsymmetric = decryptAsymmetric;
            _mongoHelper = mongoHelper;

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
            try
            {
                var consumerBuilder = new ConsumerBuilder<Ignore, string>(_clientConfig);
                
                using (var consumer = consumerBuilder.Build())
                {                                      
                    try
                    {                        
                        consumer.Subscribe(_topic);
                        while (!cancellationToken.IsCancellationRequested)  
                        {
                            ConsumeResult<Ignore, string> readResult = null;
                            try
                            {
                                
                                readResult = consumer.Consume(cancellationToken);

                                if (readResult.IsPartitionEOF)
                                {
                                    continue;
                                }

                                if (readResult.Message == null)
                                    continue;

                                if (!string.IsNullOrWhiteSpace(readResult?.Message?.Value?.ToString()))
                                {
                                    await _mongoHelper?.WriteToDB(readResult?.Message?.Value?.ToString());
                                }
                            }
                            catch (Exception ex) 
                            { 
                                Console.WriteLine($"Exception in Consumer while reading messages from the topic:{ex.Message}");
                            }
                            finally 
                            {
                                if (readResult != null && !readResult.IsPartitionEOF)
                                {
                                    consumer.StoreOffset(readResult);
                                }
                            }
                        }
                    }
                    catch (OperationCanceledException)
                    {                        
                        consumer.Close();
                    }
                    finally
                    {
                        consumer.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);                
            }           
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }        
    }
}
