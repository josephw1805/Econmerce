namespace Econ.Web;

public interface IBaseService
{
  Task<ResponseDto> SendAsync(RequestDto requestDto, bool withBearer = true);
}
