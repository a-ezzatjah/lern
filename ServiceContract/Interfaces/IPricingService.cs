using ServiceContract.DTO.DtoCommit;
using ServiceContract.DTO.DtoPricing;

namespace ServiceContract.Interfaces
{
    public interface IPricingService
    {
        Task<ServiceResponseDto<PricingResultDto>> CalculateAsync(PricingRequestDto model);
    }
}
