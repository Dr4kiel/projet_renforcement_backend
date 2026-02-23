using System.ComponentModel.DataAnnotations.Schema;

namespace server.Models;

public class Historian
{
    public int Id { get; set; }

    [Column("timestamp_")]
    public DateTime Timestamp { get; set; }

    [Column("value_")]
    public decimal Value { get; set; }

    [Column("tag_name")]
    public int TagNameId { get; set; }

    // Navigation property
    public Tag TagNameNavigation { get; set; } = null!;
}
