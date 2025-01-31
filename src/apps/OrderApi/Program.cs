using Acme.Application.Orders.Commands.CreateOrder;
using Acme.OrderApi;
using Acme.Persistence;
using MediatR;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddPersistenceConfiguration();

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