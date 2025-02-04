using Acme.Application.Common.Transactions;
using FluentResults;
using MediatR;

namespace Acme.Application.Orders.Commands;

public sealed record CreateOrderCommand : IRequest<Result<CreateOrderViewModel>>, ITransactionalRequest;