namespace Econ.Services.AuthAPI;

public interface IAuthService
{
  Task<string> Register(RegistrationRequestDto registrationRequestDto);
  Task<LoginResponseDto> Login(LoginRequestDto loginRequestDto);
  Task<bool> AssignRole(string email, string roleName);
}
