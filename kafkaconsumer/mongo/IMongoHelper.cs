namespace kafkaconsumer.mongo
{
    public interface IMongoHelper
    {
        public string GetProjID();
        public Task WriteToDB(string message);
    }
}