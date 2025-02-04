using System.ComponentModel.DataAnnotations;

namespace Acme.Infrastructure.Events;

public sealed class OutboxOptions
{
    [Required]
    public string TableName { get; set; } = null!;
}