using DTO;

namespace ServiceContract.DTO.DtoPricing
{
    public class PricingRequestDto
    {
        public decimal UnitPrice { get; set; }
        public decimal Quantity { get; set; } = 1;
        public decimal? DiscountValue { get; set; }
        public DisconTypeEnum? DiscountType { get; set; }
        public DateTime? DiscountStartAt { get; set; }
        public DateTime? DiscountEndAt { get; set; }
    }
}
