using System.ComponentModel.DataAnnotations;

namespace Acme.Infrastructure.Orders;

public sealed class OrderOptions
{
    [Required]
    public string TableName { get; set; } = null!;
}