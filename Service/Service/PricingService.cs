using ServiceContract.DTO.DtoCommit;
using ServiceContract.DTO.DtoPricing;
using ServiceContract.Interfaces;

namespace Service.Service
{
    public class PricingService : IPricingService
    {
        public Task<ServiceResponseDto<PricingResultDto>> CalculateAsync(PricingRequestDto model)
        {
            throw new NotImplementedException();
        }
    }
}
