using IdentityModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Econ.Web.Controllers;

public class HomeController(IProductService productService, ICartService cartService) : Controller
{
    private readonly IProductService _productService = productService;
    private readonly ICartService _cartService = cartService;

    public async Task<IActionResult> Index()
    {
        List<ProductDto> list = [];

        ResponseDto response = await _productService.GetAllProductsAsync();

        if (response != null && response.IsSuccess)
        {
            list = JsonConvert.DeserializeObject<List<ProductDto>>(Convert.ToString(response.Result)!);
        }
        else
        {
            TempData["error"] = response?.Message;
        }
        return View(list);
    }

    [Authorize]
    public async Task<IActionResult> ProductDetails(int itemid)
    {
        ProductDto model = new();
        ResponseDto response = await _productService.GetProductByIdAsync(itemid);

        if (response != null && response.IsSuccess)
        {
            model = JsonConvert.DeserializeObject<ProductDto>(Convert.ToString(response.Result)!);
        }
        else
        {
            TempData["error"] = response?.Message;
        }
        return View(model);
    }

    [Authorize]
    [HttpPost]
    [ActionName("ProductDetails")]
    public async Task<IActionResult> ProductDetails(ProductDto productDto)
    {
        CartDto cartDto = new()
        {
            CartHeader = new()
            {
                UserId = User.Claims.Where(u => u.Type == JwtClaimTypes.Subject)?.FirstOrDefault()?.Value
            }
        };

        CartDetailsDto cartDetails = new()
        {
            Count = productDto.Count,
            ProductId = productDto.ProductId
        };

        List<CartDetailsDto> cartDetailsDtos = [cartDetails];
        cartDto.CartDetails = cartDetailsDtos;

        ResponseDto response = await _cartService.UpsertCartAsync(cartDto);

        if (response != null && response.IsSuccess)
        {
            TempData["success"] = "Item has been added to the Shopping Cart";
            return RedirectToAction(nameof(Index));
        }
        else
        {
            TempData["error"] = response?.Message;
        }
        return View(productDto);
    }
}
