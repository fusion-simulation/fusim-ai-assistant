using FreeSql.DataAnnotations;

namespace FusimAiAssiant.Models;

[Table(Name = "vmom2_cases")]
public class VmomCase
{
    [Column(IsPrimary = true, IsIdentity = true)]
    public int Id { get; set; }

    public int UserId { get; set; }

    [Column(StringLength = 128)]
    public string Title { get; set; } = "Untitled";

    [Column(StringLength = 32)]
    public string Status { get; set; } = "queued";

    [Column(DbType = "text")]
    public string InputContent { get; set; } = string.Empty;

    [Column(StringLength = 260)]
    public string WorkDirectory { get; set; } = string.Empty;

    [Column(StringLength = 512)]
    public string? ErrorMessage { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
