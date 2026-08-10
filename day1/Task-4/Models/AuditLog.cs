namespace LegacyOrders.Models;

public class AuditLog
{
    public int Id { get; set; }

    public int EntityId { get; set; }

    public string EntityName { get; set; } = string.Empty;

    public string Action { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}
