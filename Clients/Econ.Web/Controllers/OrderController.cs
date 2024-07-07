using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Econ.Web;

[Authorize]
public class OrderController(IOrderService orderService) : Controller
{
  private readonly IOrderService _orderService = orderService;
  public IActionResult OrderIndex()
  {
    return View();
  }

  public async Task<IActionResult> OrderDetail(int orderId)
  {
    OrderHeaderDto orderHeaderDto = new();
    string userId = User.Claims.Where(u => u.Type == JwtRegisteredClaimNames.Sub).FirstOrDefault().Value;

    var response = await _orderService.GetOrder(orderId);
    if (response != null && response.IsSuccess)
    {
      orderHeaderDto = JsonConvert.DeserializeObject<OrderHeaderDto>(Convert.ToString(response.Result));
    }
    if (!User.IsInRole(SD.RoleAdmin) && userId != orderHeaderDto.UserId)
    {
      return NotFound();
    }
    return View(orderHeaderDto);
  }

  [HttpPost("OrderReadyForPickup")]
  public async Task<IActionResult> OrderReadyForPickup(int itemid)
  {
    var response = await _orderService.UpdateOrderStatus(itemid, SD.Status_ReadyForPickup);
    if (response != null && response.IsSuccess)
    {
      TempData["success"] = "Status updated successfully";
      return RedirectToAction(nameof(OrderDetail), new { orderId = itemid });
    }

    return View();
  }

  [HttpPost("CompleteOrder")]
  public async Task<IActionResult> CompleteOrder(int itemid)
  {
    var response = await _orderService.UpdateOrderStatus(itemid, SD.Status_Completed);
    if (response != null && response.IsSuccess)
    {
      TempData["success"] = "Status updated successfully";
      return RedirectToAction(nameof(OrderDetail), new { orderId = itemid });
    }

    return View();
  }
  [HttpPost("CancelOrder")]
  public async Task<IActionResult> CancelOrder(int itemid)
  {
    var response = await _orderService.UpdateOrderStatus(itemid, SD.Status_Cancelled);
    if (response != null && response.IsSuccess)
    {
      TempData["success"] = "Status updated successfully";
      return RedirectToAction(nameof(OrderDetail), new { orderId = itemid });
    }

    return View();
  }

  [HttpGet]
  public IActionResult GetAll(string status)
  {
    IEnumerable<OrderHeaderDto> list;
    string userId = "";
    if (!User.IsInRole(SD.RoleAdmin))
    {
      userId = User.Claims.Where(u => u.Type == JwtRegisteredClaimNames.Sub).FirstOrDefault().Value;
    }
    ResponseDto response = _orderService.GetAllOrder(userId).GetAwaiter().GetResult();
    if (response != null && response.IsSuccess)
    {
      list = JsonConvert.DeserializeObject<List<OrderHeaderDto>>(Convert.ToString(response.Result));
      switch (status)
      {
        case "approved":
          list = list.Where(u => u.Status == SD.Status_Approved);
          break;
        case "readyforpickup":
          list = list.Where(u => u.Status == SD.Status_ReadyForPickup);
          break;
        case "cancelled":
          list = list.Where(u => u.Status == SD.Status_Cancelled || u.Status == SD.Status_Refunded);
          break;
        default:
          break;
      }
    }
    else
    {
      list = [];
    }
    return Json(new { data = list });
  }
}
