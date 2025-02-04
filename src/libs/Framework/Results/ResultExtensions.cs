using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using FluentResults;
using Microsoft.AspNetCore.Mvc;

namespace Acme.Framework.Results;

public static class ResultExtensions
{
    public static JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    private static APIGatewayProxyResponse ToErrorApiGatewayProxyResponse(this IResultBase result)
    {
        return new APIGatewayProxyResponse
        {
            StatusCode = 500,
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } },
            Body = JsonSerializer.Serialize(new ProblemDetails
            {
                Title = "An error occurred",
                Detail = result.Errors[0].Message,
                Status = 500
            }, JsonSerializerOptions)
        };
    }

    public static APIGatewayProxyResponse ToApiGatewayProxyResponse<T>(this Result<T> result)
    {
        if (result.IsFailed)
        {
            return ToErrorApiGatewayProxyResponse(result);
        }

        return new APIGatewayProxyResponse
        {
            StatusCode = 200,
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } },
            Body = JsonSerializer.Serialize(result.Value, JsonSerializerOptions)
        };
    }
}