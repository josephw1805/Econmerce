using AutoMapper;

namespace Econ.Services.ProductAPI;

public class MappingConfig
{
  public static MapperConfiguration RegisterMaps()
  {
    var mappingConfig = new MapperConfiguration(config =>
    {
      config.CreateMap<ProductDto, Product>().ReverseMap();
    });
    return mappingConfig;
  }
}
