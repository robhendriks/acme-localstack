using System.ComponentModel.DataAnnotations;

namespace Acme.Infrastructure.Events.Outbox;

public sealed class OutboxOptions
{
    [Required]
    public string TableName { get; set; } = null!;
}