namespace Econ.Services.AuthAPI;

public class LoginResponseDto
{
  public UserDto? User { get; set; }
  public string? Token { get; set; }
}
