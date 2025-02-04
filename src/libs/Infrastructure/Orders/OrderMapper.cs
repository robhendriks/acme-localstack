﻿using Acme.Domain.Orders;
using Amazon.DynamoDBv2.Model;

namespace Acme.Infrastructure.Orders;

internal static class OrderMapper
{
    public static Dictionary<string, AttributeValue> ToMap(Order order) => new()
    {
        ["id"] = new AttributeValue
        {
            S = order.Id.ToString("D")
        }
    };
}