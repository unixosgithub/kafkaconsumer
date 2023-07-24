namespace kafkaconsumer.mongo
{
    public class MongoSettings : IMongoSettings
    {
        public string MongoServer { get; set; }
        public string MongoUser { get; set; }
        public string MongoPassword { get; set; }
        public string DbName { get; set; }
        public string CollectionName { get; set; }
    }
}
