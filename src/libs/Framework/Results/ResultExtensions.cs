using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using FluentResults;
using Microsoft.AspNetCore.Mvc;

namespace Acme.Framework.Results;

public abstract class HttpError(int statusCode, string statusDescription, string message) : Error(message)
{
    public readonly int StatusCode = statusCode;
    public readonly string StatusDescription = statusDescription;
}

public sealed class NotFoundError(string message)
    : HttpError(404, "Not Found", message);

public sealed class InternalServerError(string message)
    : HttpError(500, "Internal Server Error", message);

public static class ResultExtensions
{
    public static JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    private static APIGatewayProxyResponse ToErrorApiGatewayProxyResponse(this IResultBase result)
    {
        var rootCause = result.Errors.FirstOrDefault() as HttpError;
        var statusCode = rootCause?.StatusCode ?? 500;

        return new APIGatewayProxyResponse
        {
            StatusCode = statusCode,
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } },
            Body = JsonSerializer.Serialize(new ProblemDetails
            {
                Title = rootCause?.StatusDescription ?? "An error occurred",
                Detail = rootCause?.Message ?? "An error occurred",
                Status = statusCode
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