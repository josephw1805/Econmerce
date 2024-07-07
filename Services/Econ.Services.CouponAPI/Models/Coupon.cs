using System.ComponentModel.DataAnnotations;

namespace Econ.Services.CouponAPI;

public class Coupon
{
  [Key]
  public int CouponId { get; set; }
  [Required]
  public string CouponCode { get; set; } = "";
  [Required]
  public double DiscountAmount { get; set; }
  public int MinAmount { get; set; }
}
