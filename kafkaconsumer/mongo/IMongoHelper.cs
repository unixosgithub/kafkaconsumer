namespace kafkaconsumer.mongo
{
    public interface IMongoHelper
    {
        public Task WriteToDB(string message);
    }
}