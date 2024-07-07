
using Newtonsoft.Json;

namespace Econ.Services.ShoppingCartAPI;

public class CouponService(IHttpClientFactory clientFactory) : ICouponService
{
  private readonly IHttpClientFactory _httpClientFactory = clientFactory;
  public async Task<CouponDto> GetCoupon(string couponCode)
  {
    var client = _httpClientFactory.CreateClient("Coupon");
    var response = await client.GetAsync($"/api/coupon/GetByCode/{couponCode}");
    var apiContent = await response.Content.ReadAsStringAsync();
    var resp = JsonConvert.DeserializeObject<ResponseDto>(apiContent);
    if (resp != null && resp.IsSuccess)
    {
      return JsonConvert.DeserializeObject<CouponDto>(Convert.ToString(resp.Result)!)!;
    }
    return new CouponDto();
  }
}
