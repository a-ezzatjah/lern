using DTO;

namespace ServiceContract.DTO.DtoProduct
{
    public class DtoProduct
    {
        public int Id { get; set; }
        public string? Name { get; set;}
        public decimal Price { get; set;}
        public string ?Description { get; set;}
        public decimal ?Discount { get; set;}
        public bool HasDiscount { get; set; }
        public DisconTypeEnum ?DisconType {get; set;}
        public int BranchId { get; set; }
        public string? BranchName { get; set; }

    }
}
