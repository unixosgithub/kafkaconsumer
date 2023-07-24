using Confluent.Kafka;
using kafkaconsumer.kafka;
using kafkaconsumer.mongo;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;
using static Confluent.Kafka.ConfigPropertyNames;
using MongoDB.Driver;

namespace kafkaconsumer
{
    public class ConsumerService : IHostedService
    {
        private readonly ClientConfig _clientConfig;
        private readonly ConsumerBuilder<string, string> _consumerBuilder;
        private readonly Confluent.Kafka.IConsumer<string, string> _consumer;
        private readonly string _topic;
        private CancellationTokenSource _cancellationToken;
        private readonly string _mongoConnectionURI;
        private readonly MongoClientSettings _mongoClientSettings;

        private readonly string _dbName;
        private readonly string _collectionName;
        private IMongoCollection<MongoDataDocument> _mongoCollection;
        private readonly MongoClient _mongoClient;


        public ConsumerService(IConsumerSettings consumerSettings, IMongoSettings mongoSettings)
        {
            _clientConfig = new ConsumerConfig()
            {
                BootstrapServers = consumerSettings.BootstrapServers,
                SecurityProtocol = SecurityProtocol.SaslSsl,
                SaslMechanism = SaslMechanism.Plain,
                SaslUsername = consumerSettings.SaslUsername,
                SaslPassword = consumerSettings.SaslPassword,
                GroupId = consumerSettings.GroupId,
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            _topic = consumerSettings.Topic;
            _dbName = mongoSettings.DbName;
            _collectionName = mongoSettings.CollectionName;

            //Base64 decode password
            byte[] data = Convert.FromBase64String(mongoSettings?.MongoPassword);
            string mongoPassword = System.Text.Encoding.UTF8.GetString(data);

            _mongoConnectionURI = $"mongodb+srv://{mongoSettings?.MongoUser}:{mongoPassword}@{mongoSettings?.MongoServer}/?retryWrites=true&w=majority";
            _mongoClientSettings = MongoClientSettings.FromConnectionString(_mongoConnectionURI) ;
            _mongoClient = new MongoClient(_mongoClientSettings);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                try
                {
                    if ((_mongoClient != null) && !string.IsNullOrEmpty(_dbName)
                        && !string.IsNullOrEmpty(_collectionName))
                    {                        
                        _mongoCollection = _mongoClient?.GetDatabase(_dbName)?.GetCollection<MongoDataDocument>(_collectionName);
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"failed to create a mongo client:{ex.Message}");
                }

                using (var consumerBuilder = new ConsumerBuilder
                    <Ignore, string>(_clientConfig).Build())
                {
                    consumerBuilder.Subscribe(_topic);
                    var cancelToken = new CancellationTokenSource();

                    try
                    {
                        while (true)
                        {
                            var message = consumerBuilder.Consume(cancelToken.Token);
                            if (!string.IsNullOrWhiteSpace(message?.Message?.Value))
                            {
                                var doc = new MongoDataDocument() { Value = message?.Message?.Value };
                                
                                if (_mongoCollection != null)
                                {
                                    var json = Newtonsoft.Json.JsonConvert.SerializeObject(doc);
                                    await _mongoCollection.InsertOneAsync(doc);
                                }
                            }
                        }
                    }
                    catch (OperationCanceledException)
                    {                        
                        consumerBuilder.Close();
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
