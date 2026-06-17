using System.ComponentModel.DataAnnotations;

namespace BusinessObjects;

public partial class CosmeticInformation
{
    [Key]
    public string CosmeticId { get; set; } = null!;
    public string? CosmeticName { get; set; }
    public string? SkinType { get; set; }
    public string? ExpirationDate { get; set; }
    public string? CosmeticSize { get; set; }
    public decimal? DollarPrice { get; set; }
    public string? CategoryId { get; set; }

    public virtual CosmeticCategory? Category { get; set; }
}
