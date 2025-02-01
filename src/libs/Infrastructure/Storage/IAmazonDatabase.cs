using System.Net;
using Amazon.DynamoDBv2.Model;
using FluentResults;

namespace Acme.Infrastructure.Storage;

public interface IAmazonDatabase
{
    Task<Result> SaveChangesAsync(CancellationToken cancellationToken = default);
    void Put(PutItemRequest putItemRequest);

    Task<Result<GetItemResponse>> GetAsync(string tableName, string keyName, string keyValue,
        CancellationToken cancellationToken = default);
}