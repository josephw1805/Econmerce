using System.ComponentModel.DataAnnotations;

namespace Econ.Web;

public class LoginRequestDto
{
  [Required]
  public string UserName { get; set; }
  [Required]
  public string Password { get; set; }
}
