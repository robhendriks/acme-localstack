using Acme.Application.Orders.Commands.CreateOrder;
using Acme.Application.Orders.Queries.GetOrder;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Acme.OrderApi.Orders;

internal static class OrderEndpoints
{
    internal static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/orders")
            .WithTags("Orders");

        group.MapGet("/{orderId:guid}", GetAsync);
        group.MapPost("/", PostAsync);

        return endpoints;
    }

    private static async Task<IResult> GetAsync(
        Guid orderId,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetOrderQuery(orderId), cancellationToken);

        return result.IsFailed
            ? Results.NotFound()
            : Results.Ok(result.Value);
    }

    private static async Task<IResult> PostAsync(
        [FromBody] CreateOrderCommand request,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(request, cancellationToken);

        return result.IsFailed
            ? Results.Conflict()
            : Results.Created();
    }
}