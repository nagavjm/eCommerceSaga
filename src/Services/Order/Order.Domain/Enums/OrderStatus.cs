namespace Order.Domain.Enums;

public enum OrderStatus
{
    Pending = 1,
    StockReserved = 2,
    Paid = 3,
    Confirmed = 4,
    Cancelled = 5,
    Shipped = 6
}
