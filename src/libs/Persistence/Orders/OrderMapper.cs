using System.Globalization;
using Acme.Domain.Orders;
using Amazon.DynamoDBv2.Model;

namespace Acme.Persistence.Orders;

public class OrderMapper
{
    public static Dictionary<string, AttributeValue> ToMap(Order order) => new()
    {
        ["id"] = new AttributeValue
        {
            S = order.Id.ToString("D", CultureInfo.InvariantCulture)
        },
        ["arrivalDate"] = new AttributeValue
        {
            S = order.ArrivalDate.ToString("O", CultureInfo.InvariantCulture)
        },
        ["departureDate"] = new AttributeValue
        {
            S = order.DepartureDate.ToString("O", CultureInfo.InvariantCulture)
        },
        ["adults"] = new AttributeValue
        {
            N = order.Adults.ToString("D", CultureInfo.InvariantCulture)
        },
        ["children"] = new AttributeValue
        {
            N = order.Children.ToString("D", CultureInfo.InvariantCulture)
        }
    };

    public static Order FromMap(Dictionary<string, AttributeValue> map)
        =>
            new(
                Guid.Parse(map["id"].S, CultureInfo.InvariantCulture),
                DateTime.Parse(map["arrivalDate"].S, CultureInfo.InvariantCulture),
                DateTime.Parse(map["departureDate"].S, CultureInfo.InvariantCulture),
                uint.Parse(map["adults"].N, NumberStyles.Number, CultureInfo.InvariantCulture),
                uint.Parse(map["children"].N, NumberStyles.Number, CultureInfo.InvariantCulture)
            );
}