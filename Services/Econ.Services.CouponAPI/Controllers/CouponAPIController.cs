using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Econ.Services.CouponAPI;

[Route("api/coupon")]
[ApiController]
[Authorize]
public class CouponAPIController(AppDbContext db, IMapper mapper) : ControllerBase
{
  private readonly AppDbContext _db = db;
  private readonly IMapper _mapper = mapper;
  private readonly ResponseDto _response = new();

  [HttpGet]
  public ResponseDto Get()
  {
    try
    {
      IEnumerable<Coupon> objList = [.. _db.Coupons];
      _response.Result = _mapper.Map<IEnumerable<CouponDto>>(objList);
    }
    catch (Exception ex)
    {
      _response.IsSuccess = false;
      _response.Message = ex.Message;
    }
    return _response;
  }

  [HttpGet]
  [Route("{id:int}")]
  public ResponseDto Get(int id)
  {
    try
    {
      Coupon obj = _db.Coupons.First(u => u.CouponId == id);
      _response.Result = _mapper.Map<CouponDto>(obj);
    }
    catch (Exception ex)
    {
      _response.IsSuccess = false;
      _response.Message = ex.Message;
    }
    return _response;
  }

  [HttpGet]
  [Route("GetByCode/{code}")]
  public ResponseDto GetByCode(string code)
  {
    try
    {
      Coupon obj = _db.Coupons.First(u => u.CouponCode.Equals(code));
      _response.Result = _mapper.Map<CouponDto>(obj);
    }
    catch (Exception ex)
    {
      _response.IsSuccess = false;
      _response.Message = ex.Message;
    }
    return _response;
  }

  [HttpPost]
  [Authorize(Roles = "ADMIN")]
  public ResponseDto Post([FromBody] CouponDto couponDto)
  {
    try
    {
      Coupon coupon = _mapper.Map<Coupon>(couponDto);
      _db.Coupons.Add(coupon);
      _db.SaveChanges();

      var options = new Stripe.CouponCreateOptions
      {
        Name = couponDto.CouponCode,
        Currency = "usd",
        Id = couponDto.CouponCode,
        AmountOff = (long)(couponDto.DiscountAmount * 100),
      };
      var service = new Stripe.CouponService();
      service.Create(options);
      _response.Result = _mapper.Map<CouponDto>(coupon);
    }
    catch (Exception ex)
    {
      _response.IsSuccess = false;
      _response.Message = ex.Message;
    }
    return _response;
  }

  [HttpPut]
  [Authorize(Roles = "ADMIN")]
  public ResponseDto Put([FromBody] CouponDto obj)
  {
    try
    {
      Coupon coupon = _mapper.Map<Coupon>(obj);
      _db.Coupons.Update(coupon);
      _db.SaveChanges();
      _response.Result = _mapper.Map<CouponDto>(coupon);
    }
    catch (Exception ex)
    {
      _response.IsSuccess = false;
      _response.Message = ex.Message;
    }
    return _response;
  }

  [HttpDelete]
  [Route("{id:int}")]
  [Authorize(Roles = "ADMIN")]
  public ResponseDto Delete(int id)
  {
    try
    {
      Coupon coupon = _db.Coupons.First(u => u.CouponId == id);
      _db.Coupons.Remove(coupon);
      _db.SaveChanges();
      var service = new Stripe.CouponService();
      service.Delete(coupon.CouponCode);
    }
    catch (Exception ex)
    {
      _response.IsSuccess = false;
      _response.Message = ex.Message;
    }
    return _response;
  }
}
