using Confluent.Kafka;
using Confluent.Kafka.SyncOverAsync;
using Confluent.SchemaRegistry.Serdes;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IConsumer<string, Order>>(sp =>
{
    var config = builder.Configuration.GetSection("Consumer").Get<ConsumerConfig>();
    return new ConsumerBuilder<string, Order>(config)
        .SetValueDeserializer(new JsonDeserializer<Order>().AsSyncOverAsync()).Build();
});

builder.Services.AddHostedService<Worker>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("hi", () => Results.Ok("here it is"));


app.Run();