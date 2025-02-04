namespace Acme.Persistence.Common.Storage;

public static class AmazonDbExtensions
{
    public static async Task SaveChangesOrThrowAsync(this IAmazonDb amazonDb, CancellationToken cancellationToken = default)
    {
        var saveResult = await amazonDb.SaveChangesAsync(cancellationToken);
        if (saveResult.IsFailed)
        {
            throw new InvalidOperationException($"AmazonDb transaction failed: {saveResult.Errors[0].Message}");
        }
    }
}