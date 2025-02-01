namespace Acme.Domain.Orders;

public sealed record Order(Guid Id, DateTime ArrivalDate, DateTime DepartureDate, uint Adults, uint Children);