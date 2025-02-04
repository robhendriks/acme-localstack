using Acme.Framework.Results;
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
            return new InternalServerError("Unable to retrieve order")
                .CausedBy(result.Errors);
        }

        if (result.Value == null)
        {
            return new NotFoundError("Order not found");
        }

        return new GetOrderViewModel
        {
            Id = result.Value.Id
        };
    }
}