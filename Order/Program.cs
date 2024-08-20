using Confluent.Kafka;
using Confluent.Kafka.SyncOverAsync;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<ISchemaRegistryClient>(new CachedSchemaRegistryClient(
    builder.Configuration.GetSection("SchemaRegistry").Get<SchemaRegistryConfig>()
));
builder.Services.AddSingleton<IProducer<string,Order>>(sp =>
{
    var config = builder.Configuration.GetSection("Producer").Get<ProducerConfig>();
    var schemaRegistry = sp.GetRequiredService<ISchemaRegistryClient>();
    var producer = new ProducerBuilder<string,Order>(config)
        .SetValueSerializer(new JsonSerializer<Order>(schemaRegistry).AsSyncOverAsync())
        .SetLogHandler((_, message) =>
            Console.WriteLine($"Facility: {message.Facility}--{message.Level} Message: {message.Message}"))
        .SetErrorHandler((_, error) =>
            Console.WriteLine($"Error: {error.Reason}. isFatal: {error.IsFatal}"))
        .Build();
    return producer;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var producer = app.Services.GetRequiredService<IProducer<string,Order>>();
// producer.InitTransactions(TimeSpan.FromSeconds(5));
app.MapPost("order", 
async (Order order, ILogger<Program> logger) =>
{
    const string topic = "orders";
    // producer.InitTransactions(TimeSpan.FromSeconds(10));
    // producer.BeginTransaction();

    try
    {
        var message = new Message<string, Order> { Key = order.OrderId.ToString(), Value = order };
        // use delivery report for Outbox
        Action<DeliveryReport<string, Order>> deliveryReport = (delivery) =>
            logger.LogInformation($"order {delivery.Message.Value.OrderId} status is {delivery.Status}");

        producer.Produce(topic, message, deliveryReport);
        // producer.CommitTransaction(TimeSpan.FromSeconds(10));
        producer.Flush();

        await Task.CompletedTask;
        return Results.Created("", order.OrderId);
    }
    catch (ProduceException<string,Order> e)
    {
        logger.LogInformation($"Message produce failure {e.Error.Reason}");
        // producer.AbortTransaction();
        return Results.BadRequest();
    }
    catch (KafkaException e)
    {
        logger.LogInformation($"Kafka error {e.Error.Reason}");
        // producer.AbortTransaction();
        return Results.BadRequest();
    }
});

app.Run();
