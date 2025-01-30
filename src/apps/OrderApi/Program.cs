using Acme.Application.Orders.Commands.CreateOrder;
using Acme.OrderApi;
using Amazon;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using MediatR;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddSystemsManager(cfg =>
    {
        cfg.Prefix = "OrderTable";
        cfg.Path = "/OrderTable/";
        cfg.ReloadAfter = TimeSpan.FromMinutes(5);
    });

builder.Services
    .AddAWSLambdaHosting(LambdaEventSource.RestApi)
    .AddOrderApiServices();

var app = builder.Build();

app.MapPost("/v1/orders",
    async ([FromServices] ISender sender, CancellationToken cancellationToken) =>
    {
        var result = await sender.Send(new CreateOrderCommand(), cancellationToken);

        return result.IsFailed
            ? Results.Conflict()
            : Results.Created();
    });

await app.RunAsync();