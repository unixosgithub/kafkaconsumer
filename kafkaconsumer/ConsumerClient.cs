using Confluent.Kafka;
using kafkaconsumer.Crypt;
using kafkaconsumer.Kafka;
using kafkaconsumer.mongo;
using Microsoft.AspNetCore.SignalR.Protocol;
using System.Threading;
using logging = kafkaconsumer.Logging;

namespace kafkaconsumer
{
    public class ConsumerClient : IConsumerClient, IDisposable
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly IMongoHelper _mongoHelper;
        private readonly logging.ILogger _logger;
        private readonly string _projId;

        public ConsumerClient(IMongoHelper mongoHelper, logging.ILogger logger) 
        {
            _mongoHelper = mongoHelper;
            _logger = logger;
        }

        public void Close()
        {
            _cancellationTokenSource.Cancel();
        }

        public Task Initialize(ClientConfig config, string topic, CancellationToken cancellationToken)
        {            
            return ProcessMessages(new ConsumerBuilder<Ignore, string>(config), topic, cancellationToken);
        }

        public Task ProcessMessages(ConsumerBuilder<Ignore, string> builder, string topic, CancellationToken cancellationToken)
        {
            return Task.Run(async () =>
            {
                try
                {
                    using (var consumer = builder.Build())
                    {
                        try
                        {
                            consumer.Subscribe(topic);
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
                                        await _logger?.LogInfo(_mongoHelper?.GetProjID(), "Writing Document to MongoDB");
                                        await _mongoHelper?.WriteToDB(readResult?.Message?.Value?.ToString());
                                    }
                                }
                                catch (Exception ex)
                                {
                                    await _logger.LogError(_mongoHelper?.GetProjID(), $"Exception in Consumer while reading messages from the topic:{ex.Message}");
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

            }, cancellationToken);
            
        }

        public void Dispose()
        {
            Close();
            _cancellationTokenSource.Dispose();
        }
    }

}
