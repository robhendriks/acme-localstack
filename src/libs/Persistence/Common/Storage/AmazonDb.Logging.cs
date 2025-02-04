using System.Net;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Logging;

namespace Acme.Persistence.Common.Storage;

internal sealed partial class AmazonDb
{
    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Save changes [PUT={PutCount}, UPDATE={UpdateCount}, DELETE={DeleteCount}]")
    ]
    public static partial void LogSaveChanges(ILogger logger, int putCount, int updateCount, int deleteCount);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Commit DynamoDB transaction ({Count})")
    ]
    public static partial void LogTransaction(ILogger logger, int count);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "DynamoDB transaction failed with status code ({StatusCode}): {@Response}")
    ]
    public static partial void LogFailedTransaction(
        ILogger logger,
        HttpStatusCode statusCode,
        TransactWriteItemsResponse response);
}