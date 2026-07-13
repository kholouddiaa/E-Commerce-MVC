using ECommerce.BLL.Mappings;
using ECommerce.BLL.Services;
using ECommerce.BLL.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.BLL.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddBusinessServices(this IServiceCollection services)
    {
        services.AddAutoMapper(configuration => configuration.AddProfile<MappingProfile>());
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICategoryService, CategoryService>();

        return services;
    }
}
