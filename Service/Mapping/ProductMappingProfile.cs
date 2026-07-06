using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Entities;
using Microsoft.Identity.Client;
using ServiceContract.DTO.DtoProduct;

namespace Service.Mapping
{
    public class ProductMappingProfile : Profile
    {
        public ProductMappingProfile()
        {
            CreateMap<DtoproductAdd, Product>();

            CreateMap<Product, DtoProductAdminList>();

            CreateMap<DtoProductUpdate, Product>()
                .ForMember(x => x.Id, y => y.Ignore())
                .ForMember(x => x.Name, otp => { otp.PreCondition(s => s.Name != null); otp.MapFrom(s => s.Name.Trim()); })
                .ForMember(x => x.Description, otp => { otp.PreCondition(s => s.Description != null); otp.MapFrom(s => s.Description); });
        }
    }
}
