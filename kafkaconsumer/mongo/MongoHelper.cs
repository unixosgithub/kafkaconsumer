using kafkaconsumer.Crypt;
using kafkaconsumer.Kafka;
using MongoDB.Driver;

namespace kafkaconsumer.mongo
{
    public class MongoHelper : IMongoHelper
    {
        private readonly IDecryptAsymmetric _decryptAsymmetric;
        private readonly string _mongoConnectionURI;
        private readonly MongoClientSettings _mongoClientSettings;
        private readonly string _dbName;
        private readonly string _collectionName;
        private IMongoCollection<MongoDataDocument> _mongoCollection;
        private readonly MongoClient _mongoClient;
        private readonly IMongoSettings _mongoSettings;
        private readonly ICryptoSettings _cryptoSettings;

        public MongoHelper(IConfiguration config, IDecryptAsymmetric decryptAsymmetric)
        {
            _mongoSettings = config?.GetSection("MongoSettings")?.Get<MongoSettings>();

            _decryptAsymmetric = decryptAsymmetric;

            _dbName = _mongoSettings?.DbName;
            _collectionName = _mongoSettings?.CollectionName;

            // Decrypt the password
            string mongoPassword = string.Empty;
            _cryptoSettings = _decryptAsymmetric?.GetConfigSettings();
            if ((_cryptoSettings != null))
            {                
                byte[] mongocipherText = Convert.FromBase64String(_mongoSettings?.MongoPassword);
                if (mongocipherText?.Length > 0)
                {
                    var decryptedPass = _decryptAsymmetric?.DecryptAsymmetricString(mongocipherText);
                    mongoPassword = decryptedPass?.Replace("\n", string.Empty);
                }
               
            }
            
            _mongoConnectionURI = $"mongodb+srv://{_mongoSettings?.MongoUser}:{mongoPassword}@{_mongoSettings?.MongoServer}/?retryWrites=true&w=majority";
            _mongoClientSettings = MongoClientSettings.FromConnectionString(_mongoConnectionURI);
            _mongoClient = new MongoClient(_mongoClientSettings);

            // try to get the mongo collection here
            try
            {
                if ((_mongoClient != null) && !string.IsNullOrEmpty(_dbName)
                    && !string.IsNullOrEmpty(_collectionName))
                {
                    _mongoCollection = _mongoClient?.GetDatabase(_dbName)?.GetCollection<MongoDataDocument>(_collectionName);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"failed to create a mongo client:{ex.Message}");
            }
        }

        public string GetProjID()
        {
            return _cryptoSettings?.ProjId ?? string.Empty;
        }
        public async Task WriteToDB(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                var doc = new MongoDataDocument() { Value = message };

                try
                {
                    if (_mongoCollection != null)
                    {
                        var json = Newtonsoft.Json.JsonConvert.SerializeObject(doc);
                        await _mongoCollection.InsertOneAsync(doc);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception while inserting doc in DB:{ex.Message}");
                }
                
            }
        }
    }
}
