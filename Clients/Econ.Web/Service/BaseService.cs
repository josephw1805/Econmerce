using System.Net;
using System.Text;
using Newtonsoft.Json;
using static Econ.Web.SD;

namespace Econ.Web;

public class BaseService(IHttpClientFactory httpClientFactory, ITokenProvider tokenProvider) : IBaseService
{
  private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
  private readonly ITokenProvider _tokenProvider = tokenProvider;

  public async Task<ResponseDto> SendAsync(RequestDto requestDto, bool withBearer = true)
  {
    try
    {
      HttpClient client = _httpClientFactory.CreateClient("EconAPI");
      HttpRequestMessage message = new();
      if (requestDto.ContentType == ContentType.MultipartFormData)
      {
        message.Headers.Add("Accept", "*/*");
      }
      else
      {
        message.Headers.Add("Accept", "Application/json");
      }

      if (withBearer)
      {
        var token = _tokenProvider.GetToken();
        message.Headers.Add("Authorization", $"Bearer {token}");
      }

      message.RequestUri = new Uri(requestDto.Url);

      if (requestDto.ContentType == ContentType.MultipartFormData)
      {
        var content = new MultipartFormDataContent();

        foreach (var prop in requestDto.Data.GetType().GetProperties())
        {
          var value = prop.GetValue(requestDto.Data);
          if (value is FormFile file)
          {
            if (file != null)
            {
              content.Add(new StreamContent(file.OpenReadStream()), prop.Name, file.FileName);
            }
          }
          else
          {
            content.Add(new StringContent(value == null ? "" : value.ToString()), prop.Name);
          }
        }
        message.Content = content;
      }
      else
      {
        if (requestDto.Data != null)
        {
          message.Content = new StringContent(JsonConvert.SerializeObject(requestDto.Data), Encoding.UTF8, "application/json");
        }
      }

      HttpResponseMessage apiResponse = null;

      message.Method = requestDto.ApiType switch
      {
        ApiType.POST => HttpMethod.Post,
        ApiType.PUT => HttpMethod.Put,
        ApiType.DELETE => HttpMethod.Delete,
        _ => HttpMethod.Get,
      };

      apiResponse = await client.SendAsync(message);

      switch (apiResponse.StatusCode)
      {
        case HttpStatusCode.NotFound:
          return new() { IsSuccess = false, Message = "Not Found" };
        case HttpStatusCode.Unauthorized:
          return new() { IsSuccess = false, Message = "Unauthorized" };
        case HttpStatusCode.Forbidden:
          return new() { IsSuccess = false, Message = "Access Denied" };
        case HttpStatusCode.InternalServerError:
          return new() { IsSuccess = false, Message = "Internal Server Error" };
        default:
          var apiContent = await apiResponse.Content.ReadAsStringAsync();
          var apiResponseDto = JsonConvert.DeserializeObject<ResponseDto>(apiContent);
          return apiResponseDto;
      }
    }
    catch (Exception ex)
    {
      var dto = new ResponseDto
      {
        Message = ex.Message.ToString(),
        IsSuccess = false
      };
      return dto;
    }
  }
}
