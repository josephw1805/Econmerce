namespace Econ.Services.ShoppingCartAPI;

public interface ICouponService
{
  Task<CouponDto> GetCoupon(string couponCode);
}
