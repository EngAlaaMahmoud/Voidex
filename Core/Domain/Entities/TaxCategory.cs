using Domain.Common;

namespace Domain.Entities;
public class TaxCategory : BaseEntity
{
    public string Code { get; set; } = null!;           // e.g. "VA", "TB", "WH", "ST", "EN", "SE", ...
    public string NameAr { get; set; } = null!;
    public string? NameEn { get; set; }
    public int? SortOrder { get; set; }
    public string? Description { get; set; }

    public virtual ICollection<Tax> Taxes { get; set; } = new List<Tax>();
}