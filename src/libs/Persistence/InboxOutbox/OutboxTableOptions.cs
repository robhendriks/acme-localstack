using System.ComponentModel.DataAnnotations;

namespace Acme.Persistence.InboxOutbox;

public sealed class OutboxTableOptions
{
    [Required]
    public string TableName { get; set; } = null!;
}