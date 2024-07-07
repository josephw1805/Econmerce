using static Econ.Web.SD;

namespace Econ.Web;

public class RequestDto
{
  public ApiType ApiType { get; set; } = ApiType.GET;
  public string Url { get; set; }
  public object Data { get; set; }
  public string AccessToken { get; set; }
  public ContentType ContentType { get; set; } = ContentType.Json;
}
