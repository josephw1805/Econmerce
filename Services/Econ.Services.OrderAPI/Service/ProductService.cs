
using Newtonsoft.Json;

namespace Econ.Services.OrderAPI;

public class ProductService(IHttpClientFactory clientFactory) : IProductService
{
  private readonly IHttpClientFactory _httpClientFactory = clientFactory;

  public async Task<IEnumerable<ProductDto>> GetProducts()
  {
    var client = _httpClientFactory.CreateClient("Product");
    var response = await client.GetAsync($"/api/product");
    var apiContent = await response.Content.ReadAsStringAsync();
    var resp = JsonConvert.DeserializeObject<ResponseDto>(apiContent);
    if (resp != null && resp.IsSuccess)
    {
      return JsonConvert.DeserializeObject<IEnumerable<ProductDto>>(Convert.ToString(resp.Result)!)!;
    }
    return [];
  }
}
