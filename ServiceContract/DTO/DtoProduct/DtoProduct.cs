using DTO;

namespace ServiceContract.DTO.DtoProduct
{
    public class DtoProduct
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal? DiscountValue { get; set; }
        public DisconTypeEnum? DiscountType { get; set; }
        public bool HasDiscount => DiscountValue.HasValue && DiscountValue.Value > 0;
        public int BranchId { get; set; }
        public string? BranchName { get; set; }
    }
}
