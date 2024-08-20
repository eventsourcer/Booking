
using Confluent.Kafka;

public class Worker(IConsumer<string, Order> consumer, ILogger<Worker> logger) : BackgroundService
{
    private const string _topic = "orders";
    private readonly ILogger<Worker> _logger = logger;
    private readonly IConsumer<string, Order> _consumer = consumer;
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // ExecuteAsync blocks until a task is returned since it's started with the same thread
        // that starts the app host, therefore Task.Run or await Task.Delay(5) to return a task
        await Task.Run(async () =>
        {
            try
            {
                _consumer.Subscribe(_topic);
                _logger.LogInformation($"Listening for messages......");
                while (!stoppingToken.IsCancellationRequested)
                {
                    var consumeResult = _consumer.Consume(stoppingToken);
                    _logger.LogInformation($"{DateTime.Now} order id {consumeResult.Message.Value.OrderId} with offset {consumeResult.Offset} received.");

                    await ProcessAsync(consumeResult);
                    _consumer.StoreOffset(consumeResult);
                    _consumer.Commit();
                }
            }
            catch (ConsumeException e)
            {
                _logger.LogError($"Consume failure: {e.Error.Reason}");
            }
            catch(KafkaException e)
            {
                _logger.LogError($"Kafka error: {e.Error.Reason}");
            }
            finally
            {
                _consumer.Close();
            }
        }, stoppingToken);
    }
    private async Task ProcessAsync(ConsumeResult<string, Order> consumeResult)
    {
        await Task.Delay(5000);
        _logger.LogInformation($"{DateTime.Now} order id {consumeResult.Message.Value.OrderId} with offset {consumeResult.Offset} shipped.");
    }
    public override void Dispose()
    {
        _logger.LogInformation("Default timeout of the background service");
        _consumer.Dispose();
        base.Dispose();
    }
}