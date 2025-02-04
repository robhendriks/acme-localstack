using Acme.Infrastructure.Orders;
using FluentResults;
using MediatR;

namespace Acme.Application.Orders.Queries;

public sealed class GetOrderQueryHandler(IOrderRepo orderRepo)
    : IRequestHandler<GetOrderQuery, Result<GetOrderViewModel>>
{
    public async Task<Result<GetOrderViewModel>> Handle(GetOrderQuery request, CancellationToken cancellationToken)
    {
        var result = await orderRepo.GetAsync(request.OrderId, cancellationToken);

        if (result.IsFailed)
        {
            return Result.Fail($"Failed to get order '{request.OrderId}'")
                .WithErrors(result.Errors);
        }

        return new GetOrderViewModel
        {
            Id = result.Value.Id
        };
    }
}