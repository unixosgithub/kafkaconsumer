using Confluent.Kafka;
using kafkaconsumer;
using static Confluent.Kafka.ConfigPropertyNames;
using kafkaconsumer.kafka;
using kafkaconsumer.mongo;

var builder = WebApplication.CreateBuilder(args);

var kafkaSettings = builder.Configuration.GetSection("KafkaSettings").Get<ConsumerSettings>();
if (kafkaSettings == null)
{
    throw new ArgumentNullException(nameof(kafkaSettings));
}

var mongoSettings = builder.Configuration.GetSection("MongoSettings").Get<MongoSettings>();
if (mongoSettings == null)
{
    throw new ArgumentNullException(nameof(mongoSettings));
}

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IHostedService, ConsumerService>(x => new ConsumerService(kafkaSettings, mongoSettings));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
