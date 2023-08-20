using Confluent.Kafka;
using kafkaconsumer;
using static Confluent.Kafka.ConfigPropertyNames;
using kafkaconsumer.Kafka;
using kafkaconsumer.mongo;
using kafkaconsumer.Crypt;

var builder = WebApplication.CreateBuilder(args);
builder.Host.ConfigureAppConfiguration((context, config) =>
{
    //config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);
    //config.AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: false);
    config.AddJsonFile("config/appsettings.k8s.json", optional: true, reloadOnChange: false);
});

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IConsumerSettings, ConsumerSettings>();
builder.Services.AddSingleton<IMongoSettings, MongoSettings>();
builder.Services.AddSingleton<IDecryptAsymmetric, DecryptAsymmetric>();
builder.Services.AddSingleton<IMongoHelper, MongoHelper>();
builder.Services.AddSingleton<IHostedService, ConsumerService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
