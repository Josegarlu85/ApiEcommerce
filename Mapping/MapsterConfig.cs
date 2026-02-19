using ApiEcommerce.Models;
using ApiEcommerce.Models.Dtos;
using Mapster;

namespace ApiEcommerce.Mapping;

public static class MapsterConfig
{
    public static void RegisterMappings()
    {
        var config = TypeAdapterConfig.GlobalSettings;

        // CATEGORY
        config.NewConfig<Category, CategoryDto>();
        config.NewConfig<Category, CreateCategoryDto>();

        // PRODUCT
        config.NewConfig<Product, ProductDto>()
            .Map(dest => dest.CategoryName, src => src.Category.Name);

        config.NewConfig<Product, CreateProductDto>();
        config.NewConfig<Product, UpdateProductDto>();

        // USER
        config.NewConfig<User, UserDto>();
        config.NewConfig<User, CreateUserDto>();
        config.NewConfig<User, UserLoginDto>();
        config.NewConfig<User, UserLoginResponseDto>();

        config.NewConfig<ApplicationUser, UserDataDto>();
        config.NewConfig<ApplicationUser, UserDto>();
    }
}
