using Acme.Persistence.Common.Storage;
using FluentResults;
using MediatR;

namespace Acme.Application.Common.Transactions;

public sealed class TransactionalBehavior<TRequest, TResponse>(IAmazonDb amazonDb)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ITransactionalRequest
    where TResponse : IResultBase

{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var response = await next();

        if (response.IsFailed)
        {
            return response;
        }

        var saveResult = await amazonDb.SaveChangesAsync(cancellationToken);
        if (saveResult.IsFailed)
        {
            // TODO: Ensure failure
            // response.IsFailed = true;
        }

        return response;
    }
}