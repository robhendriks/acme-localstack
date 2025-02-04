using System.Net;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace Acme.Persistence.Common.Storage;

internal sealed partial class AmazonDb(IAmazonDynamoDB dynamoDb, ILogger<AmazonDb> logger) : IAmazonDb
{
    private readonly List<PutItemRequest> _putItemRequests = [];

    public void Put(PutItemRequest putItemRequest)
    {
        _putItemRequests.Add(putItemRequest);
    }

    public async Task<Result> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var request = new TransactWriteItemsRequest();

        LogSaveChanges(logger, _putItemRequests.Count, 0, 0);

        request.TransactItems.AddRange(
            _putItemRequests.Select(put => new TransactWriteItem
            {
                Put = new Put { Item = put.Item, TableName = put.TableName }
            })
        );

        LogTransaction(logger, request.TransactItems.Count);

        var response = await dynamoDb.TransactWriteItemsAsync(request, cancellationToken);

        if (response.HttpStatusCode != HttpStatusCode.OK)
        {
            LogFailedTransaction(logger, response.HttpStatusCode, response);
            return Result.Fail($"DynamoDb transaction failed with status code {response.HttpStatusCode}");
        }

        _putItemRequests.Clear();

        return Result.Ok();
    }
}