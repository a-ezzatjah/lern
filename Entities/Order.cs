namespace Entities;

public class Order
{
    public int Id { get; set; }
    public string CustomerKey { get; set; } = null!;
    public string CustomerFirstName { get; set; } = null!;
    public string CustomerLastName { get; set; } = null!;
    public string CustomerPhone { get; set; } = null!;
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public decimal Subtotal { get; set; }
    public decimal Total { get; set; }
    public decimal CouponDiscount { get; set; }
    public string? PaymentReference { get; set; }
    public string? ShippingProvince { get; set; }
    public string? ShippingCity { get; set; }
    public string? ShippingAddress { get; set; }
    public string? ShippingPostalCode { get; set; }
    public string? ShippingReceiver { get; set; }
    public string? ShippingPhone { get; set; }
    public string? ShippingMethod { get; set; }
    public string? ShippingCarrier { get; set; }
    public string? ShippingTrackingCode { get; set; }
    public bool ShippingPayOnDelivery { get; set; }
    public decimal ShippingCost { get; set; }
    public Guid? CheckoutToken { get; set; }
    public DateTime? ReservationExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public ICollection<PaymentTransaction> Transactions { get; set; } = new List<PaymentTransaction>();
}
