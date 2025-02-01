using Acme.FargateExample;
using Amazon.SQS;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddLogging()
    .AddHealthChecks();

builder.Services
    .AddSingleton<IAmazonSQS>(_ => new AmazonSQSClient())
    .AddHostedService<ConsumerService>();

var app = builder.Build();

app.UseHealthChecks("/health");

app.MapGet("/", () => "Hello World!");

await app.RunAsync();