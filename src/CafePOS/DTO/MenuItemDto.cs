namespace CafePOS.DTO;

public sealed record MenuItemDto(int Id, string Name, int CategoryId, string Category, decimal Price);
public sealed record OrderLineDto(int ItemId, string Name, int Quantity, decimal UnitPrice)
{
    public decimal Amount => Quantity * UnitPrice;
}
