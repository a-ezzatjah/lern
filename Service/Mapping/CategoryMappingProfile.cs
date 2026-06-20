using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Entities;
using Entities.Migrations;
using ServiceContract.DTO.DtoCategory;

namespace Service.Mapping
{
    class CategoryMappingProfile : Profile
    {
        
        public CategoryMappingProfile()
        {
            CreateMap<addcategory, Category>();
            CreateMap<Category, DtoCategory>().ReverseMap();

            CreateMap<DtoCategoryUpdate, Category>()
                .ForMember(x => x.Id, y => y.Ignore())
                .ForMember(x => x.Name, otp => { otp.PreCondition(s => !string.IsNullOrWhiteSpace(s.Name)); otp.MapFrom(s => s.Name.Trim()); });
                
            

        }



    }
}
