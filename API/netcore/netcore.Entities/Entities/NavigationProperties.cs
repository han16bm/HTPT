namespace netcore.Entities.Entities;

public partial class Product
{
    public Category? Category { get; set; }
    public ICollection<ProductImage> ProductImages { get; set; } = [];
}

public partial class InventoryTransaction
{
    public Product? Product { get; set; }
}

public partial class CartItem
{
    public Product? Product { get; set; }
}

public partial class OrderItem
{
    public Product? Product { get; set; }
    public Order? Order { get; set; }
}

public partial class Order
{
    public ICollection<OrderItem> OrderItems { get; set; } = [];
}

public partial class BlogPost
{
    public BlogCategory? BlogCategory { get; set; }
}
