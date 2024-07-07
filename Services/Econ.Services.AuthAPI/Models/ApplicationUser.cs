using Microsoft.AspNetCore.Identity;

namespace Econ.Services.AuthAPI;

public class ApplicationUser : IdentityUser
{
  public string? Name { get; set; }
}
