using Amazon.DynamoDBv2.Model;
using FluentResults;

namespace Acme.Persistence.Common.Storage;

public interface IAmazonDb
{
    void Put(PutItemRequest putItemRequest);
    Task<Result> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<Result<GetItemResponse>> GetAsync(GetItemRequest request, CancellationToken cancellationToken = default);
}