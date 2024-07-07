namespace Econ.Services.ShoppingCartAPI;

public class CartDto
{
  public required CartHeaderDto CartHeader { get; set; }
  public IEnumerable<CartDetailsDto>? CartDetails { get; set; }
}
