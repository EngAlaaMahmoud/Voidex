using Domain.Common;

namespace Domain.Entities;

public class ProductCompany : BaseEntity
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Street { get; set; }
    public string? City { get; set; }

    public ICollection<ProductGroup>? ProductGroups { get; set; } = new List<ProductGroup>();
}
