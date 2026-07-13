using AutoMapper;
using ECommerce.BLL.DTOs.Categories;
using ECommerce.BLL.DTOs.Products;
using ECommerce.DAL.Entities;

namespace ECommerce.BLL.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Category, CategoryDto>();
        CreateMap<Category, CategoryUpsertDto>().ReverseMap();

        CreateMap<Product, ProductDto>()
            .ForMember(destination => destination.CategoryName,
                options => options.MapFrom(source => source.Category != null ? source.Category.Name : string.Empty));

        CreateMap<Product, ProductUpsertDto>().ReverseMap();
    }
}
