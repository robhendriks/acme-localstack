using System.Net;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using FluentResults;

namespace Acme.Infrastructure.Storage;

internal sealed class AmazonDatabaseError() : Error("");

internal sealed class AmazonDatabase(IAmazonDynamoDB client) : IAmazonDatabase
{
    private readonly List<PutItemRequest> _putItemRequests = [];

    public async Task<Result> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var request = new TransactWriteItemsRequest
        {
            TransactItems = _putItemRequests.ConvertAll(x => new TransactWriteItem
                { Put = new Put { TableName = x.TableName, Item = x.Item } })
        };

        if (request.TransactItems.Count == 0)
        {
            return Result.Ok();
        }

        var response = await client.TransactWriteItemsAsync(request, cancellationToken);

        _putItemRequests.Clear();
        // TODO: update, delete

        if (response.HttpStatusCode != HttpStatusCode.OK)
        {
            return new AmazonDatabaseError();
        }

        return Result.Ok();
    }

    public void Put(PutItemRequest putItemRequest) =>
        _putItemRequests.Add(putItemRequest);
}