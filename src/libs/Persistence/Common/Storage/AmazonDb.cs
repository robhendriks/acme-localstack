using System.Net;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace Acme.Persistence.Common.Storage;

internal sealed partial class AmazonDb(IAmazonDynamoDB dynamoDb, ILogger<AmazonDb> logger) : IAmazonDb
{
    private readonly List<PutItemRequest> _putItemRequests = [];
    private readonly List<UpdateItemRequest> _updateItemRequests = [];

    public void Put(PutItemRequest putItemRequest) =>
        _putItemRequests.Add(putItemRequest);

    public void Update(UpdateItemRequest request) =>
        _updateItemRequests.Add(request);

    public async Task<Result> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var request = new TransactWriteItemsRequest();

        LogSaveChanges(logger, _putItemRequests.Count, 0, 0);

        // Process PUT operations
        request.TransactItems.AddRange(
            _putItemRequests.Select(put => new TransactWriteItem
            {
                Put = new Put { Item = put.Item, TableName = put.TableName }
            })
        );

        // Process UPDATE operations
        request.TransactItems.AddRange(
            _updateItemRequests.Select(update => new TransactWriteItem
            {
                Update = new Update
                {
                    TableName = update.TableName,
                    Key = update.Key,
                    ExpressionAttributeNames = update.ExpressionAttributeNames,
                    ExpressionAttributeValues = update.ExpressionAttributeValues,
                    UpdateExpression = update.UpdateExpression,
                }
            })
        );

        // TODO: Process deletes

        LogTransaction(logger, request.TransactItems.Count);

        var response = await dynamoDb.TransactWriteItemsAsync(request, cancellationToken);

        if (response.HttpStatusCode != HttpStatusCode.OK)
        {
            LogFailedTransaction(logger, response.HttpStatusCode, response);
            return Result.Fail($"DynamoDb transaction failed with status code {response.HttpStatusCode}");
        }

        _putItemRequests.Clear();
        _updateItemRequests.Clear();

        return Result.Ok();
    }

    public async Task<Result<GetItemResponse>> GetAsync(
        GetItemRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await dynamoDb.GetItemAsync(request, cancellationToken);

        if (response.HttpStatusCode != HttpStatusCode.OK)
        {
            return Result.Fail($"DynamoDb get item failed with status code {response.HttpStatusCode}");
        }

        return response;
    }
}

public static class AmazonDbUtil
{
    public static long CalculateTtl(TimeSpan? offset = null)
    {
        var ttl = DateTimeOffset.UtcNow;

        if (offset != null)
        {
            ttl += offset.Value;
        }

        return ttl.ToUnixTimeSeconds();
    }
}