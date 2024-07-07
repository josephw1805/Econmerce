namespace Econ.Services.OrderAPI;

public interface IProductService
{
  Task<IEnumerable<ProductDto>> GetProducts();
}
