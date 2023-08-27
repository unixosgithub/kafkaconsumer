namespace kafkaconsumer.Logging
{
    public interface ILogger
    {
        public Task LogInfo(string projectId, string message);
        public Task LogError(string projectId, string message);
    }
}
