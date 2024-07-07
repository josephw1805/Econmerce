namespace Econ.Services.ShoppingCartAPI;

public interface IProductService
{
  Task<IEnumerable<ProductDto>> GetProducts();
}
