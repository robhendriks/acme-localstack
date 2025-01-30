using System.ComponentModel.DataAnnotations;

namespace Acme.Persistence.Orders;

public sealed class OrderTableOptions
{
    [Required] public string TableName { get; set; } = null!;
}