namespace ServiceContract.DTO.DtoPricing
{
    public class PricingResultDto
    {
        public decimal UnitPrice { get; set; }
        public decimal Quantity { get; set; }
        public decimal Subtotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalPrice { get; set; }
        public bool IsDiscountActive { get; set; }
    }
}
