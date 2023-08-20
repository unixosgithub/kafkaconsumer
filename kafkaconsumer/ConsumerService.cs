using Confluent.Kafka;
using kafkaconsumer.Kafka;
using kafkaconsumer.mongo;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;
using static Confluent.Kafka.ConfigPropertyNames;
using MongoDB.Driver;
using kafkaconsumer.Crypt;

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
        private readonly IDecryptAsymmetric _decryptAsymmetric;
        private readonly IConsumerSettings _consumerSettings;
        private readonly IMongoSettings _mongoSettings;


        public ConsumerService(IConfiguration config, IDecryptAsymmetric decryptAsymmetric)
        {
            _consumerSettings = config?.GetSection("KafkaSettings")?.Get<ConsumerSettings>();
            _mongoSettings = config?.GetSection("MongoSettings")?.Get<MongoSettings>();

            _decryptAsymmetric = decryptAsymmetric;

            _clientConfig = new ConsumerConfig()
            {
                BootstrapServers = _consumerSettings.BootstrapServers,
                SecurityProtocol = SecurityProtocol.SaslSsl,
                SaslMechanism = SaslMechanism.Plain,
                SaslUsername = _consumerSettings.SaslUsername,
                SaslPassword = _consumerSettings.SaslPassword,
                GroupId = _consumerSettings.GroupId,
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            _topic = _consumerSettings.Topic;
            _dbName = _mongoSettings.DbName;
            _collectionName = _mongoSettings.CollectionName;


            // Decrypt the password
            string mongoPassword = string.Empty;
            var cryptoSettings = _decryptAsymmetric?.GetConfigSettings();
            if ((cryptoSettings != null))
            {
                //try
                //{
                byte[] cipherText = Convert.FromBase64String(_consumerSettings?.SaslPassword);
                if (cipherText?.Length > 0)
                {
                    var decryptedPass = _decryptAsymmetric?.DecryptAsymmetricString(cipherText);
                    _clientConfig.SaslPassword = decryptedPass.Replace("\n", string.Empty);
                }

                
                byte[] mongocipherText = Convert.FromBase64String(_mongoSettings?.MongoPassword);
                if (mongocipherText?.Length > 0)
                {
                    var decryptedPass = _decryptAsymmetric?.DecryptAsymmetricString(mongocipherText);
                    mongoPassword = decryptedPass.Replace("\n", string.Empty);
                }
                //}
                //catch(Exception ex)
                //{ 
                //    throw new Exception(ex);
                //}
            }
            //Base64 decode password
            //byte[] data = Convert.FromBase64String(_mongoSettings?.MongoPassword);
            //string mongoPassword = System.Text.Encoding.UTF8.GetString(data);

            _mongoConnectionURI = $"mongodb+srv://{_mongoSettings?.MongoUser}:{mongoPassword}@{_mongoSettings?.MongoServer}/?retryWrites=true&w=majority";
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
