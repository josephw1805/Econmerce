using AutoMapper;
using Econ.MessageBus;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Stripe.Checkout;

namespace Econ.Services.OrderAPI;

[Route("api/order")]
[ApiController]
public class OrderAPIController(AppDbContext db, IMapper mapper, IMessageBus messageBus, IConfiguration configuration) : ControllerBase
{
  protected ResponseDto _response = new();
  private readonly IMapper _mapper = mapper;
  private readonly AppDbContext _db = db;
  private readonly IMessageBus _messageBus = messageBus;
  private readonly IConfiguration _configuration = configuration;

  [Authorize]
  [HttpGet("GetOrders")]
  public ResponseDto? Get(string? userId = "")
  {
    try
    {
      IEnumerable<OrderHeader> objList;
      if (User.IsInRole(SD.RoleAdmin))
      {
        objList = [.. _db.OrderHeaders.Include(u => u.OrderDetails).OrderByDescending(u => u.OrderHeaderId)];
      }
      else
      {
        objList = [.. _db.OrderHeaders.Include(u => u.OrderDetails).Where(u => u.UserId == userId).OrderByDescending(u => u.OrderHeaderId)];
      }
      _response.Result = _mapper.Map<IEnumerable<OrderHeaderDto>>(objList);
    }
    catch (Exception ex)
    {
      _response.IsSuccess = false;
      _response.Message = ex.Message;
    }
    return _response;
  }

  [Authorize]
  [HttpGet("GetOrder/{id:int}")]
  public ResponseDto? Get(int id)
  {
    try
    {
      OrderHeader orderHeader = _db.OrderHeaders.Include(u => u.OrderDetails).First(u => u.OrderHeaderId == id);
      _response.Result = _mapper.Map<OrderHeaderDto>(orderHeader);
    }
    catch (Exception ex)
    {
      _response.IsSuccess = false;
      _response.Message = ex.Message;
    }
    return _response;
  }

  [Authorize]
  [HttpPost("CreateOrder")]
  public async Task<ResponseDto> CreateOrder([FromBody] CartDto cartDto)
  {
    try
    {
      OrderHeaderDto orderHeaderDto = _mapper.Map<OrderHeaderDto>(cartDto.CartHeader);
      orderHeaderDto.OrderTime = DateTime.Now;
      orderHeaderDto.Status = SD.Status_Pending;
      orderHeaderDto.OrderDetails = _mapper.Map<IEnumerable<OrderDetailsDto>>(cartDto.CartDetails);
      OrderHeader orderCreated = _db.OrderHeaders.Add(_mapper.Map<OrderHeader>(orderHeaderDto)).Entity;
      await _db.SaveChangesAsync();
      orderHeaderDto.OrderHeaderId = orderCreated.OrderHeaderId;
      _response.Result = orderHeaderDto;
    }
    catch (Exception ex)
    {
      _response.IsSuccess = false;
      _response.Message = ex.Message;
    }
    return _response;
  }

  [Authorize]
  [HttpPost("CreateStripeSession")]
  public ResponseDto CreateStripeSession([FromBody] StripeRequestDto stripeRequestDto)
  {
    try
    {
      var options = new SessionCreateOptions
      {
        SuccessUrl = stripeRequestDto.ApprovedUrl,
        CancelUrl = stripeRequestDto.CancelUrl,
        LineItems = [],
        Mode = "payment",
      };

      var discountsObj = new List<SessionDiscountOptions>()
      {
        new() {
          Coupon = stripeRequestDto.OrderHeader!.CouponCode
        }
      };

      if (stripeRequestDto.OrderHeader != null && stripeRequestDto.OrderHeader.OrderDetails != null)
      {
        foreach (var item in stripeRequestDto.OrderHeader.OrderDetails)
        {
          var sessionLineItem = new SessionLineItemOptions
          {
            PriceData = new SessionLineItemPriceDataOptions
            {
              UnitAmount = (long)(item.Price * 100),
              Currency = "usd",
              ProductData = new SessionLineItemPriceDataProductDataOptions
              {
                Name = item.Product!.Name
              }
            },
            Quantity = item.Count
          };
          options.LineItems.Add(sessionLineItem);
        }
      }
      if (stripeRequestDto.OrderHeader!.Discount > 0)
      {
        options.Discounts = discountsObj;
      }
      var service = new SessionService();
      Session session = service.Create(options);
      stripeRequestDto.StripeSessionUrl = session.Url;
      OrderHeader orderHeader = _db.OrderHeaders.First(u => u.OrderHeaderId == stripeRequestDto.OrderHeader!.OrderHeaderId);
      orderHeader.StripeSessionId = session.Id;
      _db.SaveChanges();
      _response.Result = stripeRequestDto;
    }
    catch (Exception ex)
    {
      _response.Message = ex.Message;
      _response.IsSuccess = false;
    }
    return _response;
  }

  [Authorize]
  [HttpPost("ValidateStripeSession")]
  public async Task<ResponseDto> ValidateStripeSession([FromBody] int orderHeaderId)
  {
    try
    {
      OrderHeader orderHeader = _db.OrderHeaders.First(u => u.OrderHeaderId == orderHeaderId);
      var service = new SessionService();
      Session session = service.Get(orderHeader.StripeSessionId);
      var paymentIntentService = new PaymentIntentService();
      PaymentIntent paymentIntent = paymentIntentService.Get(session.PaymentIntentId);

      if (paymentIntent.Status == "succeeded")
      {
        // then payment was successful
        orderHeader.PaymentIntentId = paymentIntent.Id;
        orderHeader.Status = SD.Status_Approved;
        _db.SaveChanges();
        RewardsDto rewardsDto = new()
        {
          OrderId = orderHeader.OrderHeaderId,
          RewardsActivity = Convert.ToInt32(orderHeader.OrderTotal),
          UserId = orderHeader.UserId
        };
        string topicName = _configuration.GetValue<string>("TopicAndQueueNames:OrderCreatedTopic")!;
        await _messageBus.PublishMessage(rewardsDto, topicName);
        _response.Result = _mapper.Map<OrderHeaderDto>(orderHeader);
      }
    }
    catch (Exception ex)
    {
      _response.Message = ex.Message;
      _response.IsSuccess = false;
    }
    return _response;
  }

  [Authorize]
  [HttpPost("UpdateOrderStatus/{orderId:int}")]
  public ResponseDto UpdateOrderStatus(int orderId, [FromBody] string newStatus)
  {
    try
    {
      OrderHeader orderHeader = _db.OrderHeaders.First(u => u.OrderHeaderId == orderId);
      if (orderHeader != null)
      {
        if (newStatus == SD.Status_Cancelled)
        {
          // refund
          var options = new RefundCreateOptions
          {
            Reason = RefundReasons.RequestedByCustomer,
            PaymentIntent = orderHeader.PaymentIntentId,
          };

          var services = new RefundService();
          Refund refund = services.Create(options);
          orderHeader.Status = newStatus;
        }
        orderHeader.Status = newStatus;
        _db.SaveChanges();
      }
    }
    catch (Exception ex)
    {
      _response.Message = ex.Message;
      _response.IsSuccess = false;
    }
    return _response;
  }
}
