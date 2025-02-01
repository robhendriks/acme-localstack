using Acme.OrderApi;
using Acme.OrderApi.Orders;
using Acme.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddPersistenceConfiguration();

builder.Services
    .AddAWSLambdaHosting(LambdaEventSource.RestApi)
    .AddOrderApiServices();

var app = builder.Build();

app.MapOrderEndpoints();

await app.RunAsync();