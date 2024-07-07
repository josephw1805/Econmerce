namespace Econ.Web;

public interface ICartService
{
  Task<ResponseDto> GetCartByUserIdAsync(string userId);
  Task<ResponseDto> UpsertCartAsync(CartDto cartDto);
  Task<ResponseDto> RemoveFromCartAsync(int cartDetailsId);
  Task<ResponseDto> ApplyCouponAsync(CartDto cartDto);
  Task<ResponseDto> EmailCart(CartDto cartDto);
}
