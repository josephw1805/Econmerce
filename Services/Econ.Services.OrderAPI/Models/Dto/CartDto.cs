namespace Econ.Services.OrderAPI;

public class CartDto
{
  public CartHeaderDto? CartHeader { get; set; }
  public IEnumerable<CartDetailsDto>? CartDetails { get; set; }
}
