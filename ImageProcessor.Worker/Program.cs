using ImageProcessor.Infrastructure.Messaging;
using ImageProcessor.Worker;
using ImageProcessor.Worker.Messaging;
using ImageProcessor.Worker.Processing;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection("RabbitMQ"));

builder.Services.AddSingleton<RabbitMqChannelFactory>();
builder.Services.AddSingleton<IProcessingQueueConsumer, RabbitMqProcessingQueueConsumer>();
builder.Services.AddSingleton<ITaskHandler, ImageProcessingHandler>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
