namespace PontelloImport.Models
{
    /// <summary>
	/// Status of an order in the order processing workflow
	/// </summary>
    public enum OrderStatus
    {
        Pending = 0,
        Processing = 1,
        Shipped = 2,
        Delivered = 3,
        Cancelled = 4
    }
}
