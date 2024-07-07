using Microsoft.AspNetCore.Identity;

namespace Econ.Services.AuthAPI;

public class AuthService(AppDbContext db, IJwtTokenGenerator jwtTokenGenerator, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager) : IAuthService
{
  private readonly AppDbContext _db = db;
  private readonly UserManager<ApplicationUser> _userManager = userManager;
  private readonly RoleManager<IdentityRole> _roleManager = roleManager;
  private readonly IJwtTokenGenerator _jwtTokenGenerator = jwtTokenGenerator;

  public async Task<bool> AssignRole(string email, string roleName)
  {
    var user = _db.ApplicationUsers.FirstOrDefault(u => u.Email!.Equals(email));
    if (user != null)
    {
      if (!_roleManager.RoleExistsAsync(roleName).GetAwaiter().GetResult())
      {
        // create role if it does not exist
        _roleManager.CreateAsync(new IdentityRole(roleName)).GetAwaiter().GetResult();
      }
      await _userManager.AddToRoleAsync(user, roleName);
      return true;
    }
    return false;
  }

  public async Task<LoginResponseDto> Login(LoginRequestDto loginRequestDto)
  {
    var user = _db.ApplicationUsers.FirstOrDefault(u => u.UserName!.Equals(loginRequestDto.UserName));

    bool isValid = await _userManager.CheckPasswordAsync(user!, loginRequestDto.Password!);

    if (user == null || !isValid)
    {
      return new LoginResponseDto() { User = null, Token = "" };
    }

    // if user was found, generate JWT Token
    var roles = await _userManager.GetRolesAsync(user);
    var token = _jwtTokenGenerator.GenerateToken(user, roles);

    UserDto userDto = new()
    {
      Email = user.Email,
      ID = user.Id,
      Name = user.Name,
      PhoneNumber = user.PhoneNumber
    };
    LoginResponseDto loginResponseDto = new()
    {
      User = userDto,
      Token = token
    };
    return loginResponseDto;
  }

  public async Task<string> Register(RegistrationRequestDto registrationRequestDto)
  {
    ApplicationUser user = new()
    {
      UserName = registrationRequestDto.Email,
      Email = registrationRequestDto.Email,
      NormalizedEmail = registrationRequestDto.Email?.ToUpper(),
      Name = registrationRequestDto.Name,
      PhoneNumber = registrationRequestDto.PhoneNumber,
    };

    try
    {
      var result = await _userManager.CreateAsync(user, registrationRequestDto.Password!);
      if (result.Succeeded)
      {
        var userToReturn = _db.ApplicationUsers.First(u => u.UserName == registrationRequestDto.Email);
        UserDto userDto = new()
        {
          Email = userToReturn.Email,
          ID = userToReturn.Id,
          Name = userToReturn.Name,
          PhoneNumber = userToReturn.PhoneNumber,
        };

        return "";
      }
      else
      {
        return result.Errors.First().Description;
      }
    }
    catch (Exception)
    {
    }
    return "Error Encountered";
  }
}
