using BuildingBlocks.Common.Primitives;

namespace Product.Domain.Entities;

public class Category : Entity<Guid>
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;

    private Category() { }

    public static Category Create(string name, string description)
    {
        return new Category
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Description = description.Trim()
        };
    }
}
