namespace kafkaconsumer.mongo
{
    public interface IMongoSettings
    {
        string MongoServer { get; set; }
        string MongoUser { get; set; }
        string MongoPassword { get; set; }
        string DbName { get; set; }
        string CollectionName { get; set; }
    }
}