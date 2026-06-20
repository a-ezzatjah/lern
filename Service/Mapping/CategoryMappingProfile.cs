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
    public class CategoryMappingProfile : Profile
    {

        public CategoryMappingProfile()
        {
            CreateMap<AddDtoCategory, Category>()
                .ForMember(s => s.Name, otp => otp.MapFrom(s => s.Name != null ? s.Name.Trim() : null))
                .ForMember(s=>s.Slug, otp => otp.MapFrom(s => s.Slug != null ? s.Slug.Trim() : null));


            CreateMap<Category, DtoCategory>().ReverseMap();

            CreateMap<DtoCategoryUpdate, Category>()
                .ForMember(x => x.Id, y => y.Ignore())
                .ForMember(x => x.Name, otp =>
                { otp.PreCondition(s => !string.IsNullOrWhiteSpace(s.Name)); otp.MapFrom(s => s.Name.Trim()); })
                .ForMember(x => x.Slug, otp =>
                { otp.PreCondition(s => !string.IsNullOrWhiteSpace(s.Slug)); otp.MapFrom(s => s.Slug.Trim()); });



        }



    }
}
