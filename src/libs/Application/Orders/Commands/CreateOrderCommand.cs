using Acme.Application.Common.Transactions;
using FluentResults;
using MediatR;

namespace Acme.Application.Orders.Commands;

public sealed record CreateOrderCommand(string Note) : IRequest<Result<CreateOrderViewModel>>, ITransactionalRequest;