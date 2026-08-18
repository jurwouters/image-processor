using ImageProcessor.Infrastructure.Extensions;
using ImageProcessor.Infrastructure.Messaging;
using ImageProcessor.Worker;
using ImageProcessor.Worker.Messaging;
using ImageProcessor.Worker.Processing;
using ImageProcessor.Worker.Processing.Operations;
using ImageProcessor.Worker.Storage;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection("RabbitMQ"));
builder.Services
    .AddData(builder.Configuration)
    .AddStorage(builder.Configuration);

builder.Services.AddSingleton<RabbitMqChannelFactory>();
builder.Services.AddSingleton<IProcessingQueueConsumer, RabbitMqProcessingQueueConsumer>();
builder.Services.AddScoped<ITaskHandler, ImageProcessingHandler>();
builder.Services.AddSingleton<IImageObjectStorage, MinioImageObjectStorage>();

builder.Services.AddSingleton<IImageOperationProcessor, CropOperationProcessor>();
builder.Services.AddSingleton<IImageOperationProcessor, ResizeOperationProcessor>();
builder.Services.AddSingleton<IImageOperationProcessorResolver, ImageOperationProcessorResolver>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
