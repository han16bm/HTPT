namespace netcore.Entities.Entities;

/// <summary>
/// Navigation properties cho Product (partial class — tách với scaffold).
/// </summary>
public partial class Product
{
    public Category? Category { get; set; }
    public ICollection<ProductImage> ProductImages { get; set; } = [];
}

/// <summary>
/// Navigation properties cho InventoryTransaction.
/// </summary>
public partial class InventoryTransaction
{
    public Product? Product { get; set; }
}

/// <summary>
/// Navigation properties cho CartItem.
/// </summary>
public partial class CartItem
{
    public Product? Product { get; set; }
}

/// <summary>
/// Navigation properties cho OrderItem.
/// </summary>
public partial class OrderItem
{
    public Product? Product { get; set; }
    public Order? Order { get; set; }
}

/// <summary>
/// Navigation properties cho Order.
/// </summary>
public partial class Order
{
    public ICollection<OrderItem> OrderItems { get; set; } = [];
}

/// <summary>
/// Navigation properties cho BlogPost.
/// </summary>
public partial class BlogPost
{
    public BlogCategory? BlogCategory { get; set; }
}


