using System.ComponentModel.DataAnnotations;

namespace Acme.Infrastructure.Events.Inbox;

internal sealed class InboxOptions
{
    [Required]
    public string TableName { get; init; } = null!;
}