using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

namespace Econ.Web;

public class AuthController(IAuthService authService, ITokenProvider tokenProvider) : Controller
{
  private readonly IAuthService _authService = authService;
  private readonly ITokenProvider _tokenProvider = tokenProvider;

  [HttpGet]
  public IActionResult Login()
  {
    LoginRequestDto loginRequestDto = new();
    return View(loginRequestDto);
  }

  [HttpPost]
  public async Task<IActionResult> Login(LoginRequestDto obj)
  {
    ResponseDto responseDto = await _authService.LoginAsync(obj);

    if (responseDto != null && responseDto.IsSuccess)
    {
      LoginResponseDto loginResponseDto = JsonConvert.DeserializeObject<LoginResponseDto>(Convert.ToString(responseDto.Result)!);
      if (loginResponseDto != null)
      {
        await SignInUser(loginResponseDto);
        _tokenProvider.SetToken(loginResponseDto.Token!);
      }
      return RedirectToAction("Index", "Home");
    }
    else
    {
      TempData["error"] = responseDto!.Message;
      return View(obj);
    }
  }

  [HttpGet]
  public IActionResult Register()
  {
    var roleList = new List<SelectListItem>()
    {
      new() {Text=SD.RoleAdmin,Value=SD.RoleAdmin},
      new() {Text=SD.RoleCustomer,Value=SD.RoleCustomer},
    };
    ViewBag.RoleList = roleList;
    return View();
  }

  [HttpPost]
  public async Task<IActionResult> Register(RegistrationRequestDto obj)
  {
    ResponseDto result = await _authService.RegisterAsync(obj);
    ResponseDto assingRole;
    if (result != null && result.IsSuccess)
    {
      if (string.IsNullOrEmpty(obj.Role))
      {
        obj.Role = SD.RoleCustomer;
      }
      assingRole = await _authService.AssignRoleAsync(obj);
      if (assingRole != null && assingRole.IsSuccess)
      {
        TempData["success"] = "Registration Successful";
        return RedirectToAction(nameof(Login));
      }
    }
    else
    {
      TempData["error"] = result!.Message;
    }
    var roleList = new List<SelectListItem>()
    {
      new() {Text=SD.RoleAdmin,Value=SD.RoleAdmin},
      new() {Text=SD.RoleCustomer,Value=SD.RoleCustomer},
    };
    ViewBag.RoleList = roleList;
    return View(obj);
  }

  [HttpGet]
  public async Task<IActionResult> Logout()
  {
    await HttpContext.SignOutAsync();
    _tokenProvider.ClearToken();
    return RedirectToAction("Index", "Home");
  }

  private async Task SignInUser(LoginResponseDto model)
  {
    var handler = new JwtSecurityTokenHandler();
    var jwt = handler.ReadJwtToken(model.Token);
    var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
    identity.AddClaim(new(JwtRegisteredClaimNames.Email, jwt.Claims.First(u => u.Type == JwtRegisteredClaimNames.Email).Value));
    identity.AddClaim(new(JwtRegisteredClaimNames.Sub, jwt.Claims.First(u => u.Type == JwtRegisteredClaimNames.Sub).Value));
    identity.AddClaim(new(JwtRegisteredClaimNames.Name, jwt.Claims.First(u => u.Type == JwtRegisteredClaimNames.Name).Value));
    identity.AddClaim(new(ClaimTypes.Name, jwt.Claims.First(u => u.Type == JwtRegisteredClaimNames.Email).Value));
    identity.AddClaim(new(ClaimTypes.Role, jwt.Claims.First(u => u.Type == "role").Value));
    var principal = new ClaimsPrincipal(identity);
    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
  }
}
