using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DTO;
using Entities;
using Microsoft.Identity.Client;

namespace Service.Mapping
{
    public class ProductMappingProfile : Profile
    {


        public ProductMappingProfile()
        {


            CreateMap<Product, DtoProduct>()
               .ForMember(x => x.BranchName, otp => otp.MapFrom(s => s.Branch.Name != null ? s.Branch.Name : null));


            CreateMap<DtoProduct, Product>();


            CreateMap<DtoProductUpdate, Product>()

                .ForMember(x => x.Id, y => y.Ignore())

                .ForMember(x => x.Name, otp => { otp.PreCondition(s => s.Name != null); otp.MapFrom(s => s.Name.Trim()); })

                .ForMember(x => x.Price, otp => { otp.PreCondition(s => s.Price > 0); otp.MapFrom(s => s.Price); })

                //.ForMember(x => x.Price, otp => { otp.PreCondition(s => s.Price.HasValue); otp.MapFrom(s => s.Price.Value); })

                .ForMember(x => x.Description, otp => { otp.PreCondition(s => s.Description != null); otp.MapFrom(s => s.Description); })

                //.ForMember(x => x.HasDiscount, otp => otp.MapFrom(s => s.HasDiscount))

                .ForMember(x => x.Discount, otp => otp.MapFrom(s => s.HasDiscount ? s.Discount : null))

                .ForMember(x => x.DisconType, otp => otp.MapFrom(s => s.HasDiscount ? s.DisconType : null));


        }


    }


}
