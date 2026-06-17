using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BusinessObjects;

public partial class CosmeticCategory
{
    [Key]
    public string CategoryId { get; set; } = null!;
    public string? CategoryName { get; set; }
    public string? UsagePurpose { get; set; }
    public string? FormulationType { get; set; }

    public virtual ICollection<CosmeticInformation> CosmeticInformations { get; set; } = new List<CosmeticInformation>();
}
