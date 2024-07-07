namespace Econ.Services.AuthAPI;

public interface IJwtTokenGenerator
{
  string GenerateToken(ApplicationUser applicationUser, IEnumerable<string> roles);
}
