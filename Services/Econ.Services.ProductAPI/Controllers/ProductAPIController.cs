using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Econ.Services.ProductAPI;

[Route("api/product")]
[ApiController]
public class ProductAPIController(AppDbContext db, IMapper mapper) : ControllerBase
{
  private readonly AppDbContext _db = db;
  private readonly IMapper _mapper = mapper;
  private readonly ResponseDto _response = new();

  [HttpGet]
  public ResponseDto Get()
  {
    try
    {
      IEnumerable<Product> objList = [.. _db.Products];
      _response.Result = _mapper.Map<IEnumerable<ProductDto>>(objList);
    }
    catch (Exception ex)
    {
      _response.IsSuccess = false;
      _response.Message = ex.Message;
    }
    return _response;
  }

  [HttpGet]
  [Route("{id:int}")]
  public ResponseDto Get(int id)
  {
    try
    {
      Product obj = _db.Products.First(u => u.ProductId == id);
      _response.Result = _mapper.Map<ProductDto>(obj);
    }
    catch (Exception ex)
    {
      _response.IsSuccess = false;
      _response.Message = ex.Message;
    }
    return _response;
  }

  [HttpPost]
  [Authorize(Roles = "ADMIN")]
  public ResponseDto Post(ProductDto productDto)
  {
    try
    {
      Product product = _mapper.Map<Product>(productDto);
      _db.Products.Add(product);
      _db.SaveChanges();

      if (productDto.Image != null)
      {
        string fileName = product.ProductId + Path.GetExtension(productDto.Image.FileName);
        string filePath = @"wwwroot/ProductImages/" + fileName;
        var filePathDirectory = Path.Combine(Directory.GetCurrentDirectory(), filePath);
        using var fileStream = new FileStream(filePathDirectory, FileMode.Create);
        productDto.Image.CopyTo(fileStream);
        var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}{HttpContext.Request.PathBase.Value}";
        product.ImageUrl = baseUrl + "/ProductImages/" + fileName;
        product.ImageLocalPath = filePath;
      }
      else
      {
        product.ImageUrl = "https://placehold.co/600x400";
      }
      _db.Products.Update(product);
      _db.SaveChanges();
      _response.Result = _mapper.Map<ProductDto>(product);
    }
    catch (Exception ex)
    {
      _response.IsSuccess = false;
      _response.Message = ex.Message;
    }
    return _response;
  }

  [HttpPut]
  [Authorize(Roles = "ADMIN")]
  public ResponseDto Put(ProductDto productDto)
  {
    try
    {
      Product product = _mapper.Map<Product>(productDto);

      if (productDto.Image != null)
      {
        if (!string.IsNullOrEmpty(product.ImageLocalPath))
        {
          var oldFilePathDirectory = Path.Combine(Directory.GetCurrentDirectory(), product.ImageLocalPath);
          FileInfo file = new(oldFilePathDirectory);
          if (file.Exists)
          {
            file.Delete();
          }
        }

        string fileName = product.ProductId + Path.GetExtension(productDto.Image.FileName);
        string filePath = @"wwwroot/ProductImages/" + fileName;
        var filePathDirectory = Path.Combine(Directory.GetCurrentDirectory(), filePath);
        using var fileStream = new FileStream(filePathDirectory, FileMode.Create);
        productDto.Image.CopyTo(fileStream);
        var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}{HttpContext.Request.PathBase.Value}";
        product.ImageUrl = baseUrl + "/ProductImages/" + fileName;
        product.ImageLocalPath = filePath;
      }

      _db.Products.Update(product);
      _db.SaveChanges();
      _response.Result = _mapper.Map<ProductDto>(product);
    }
    catch (Exception ex)
    {
      _response.IsSuccess = false;
      _response.Message = ex.Message;
    }
    return _response;
  }

  [HttpDelete]
  [Route("{id:int}")]
  [Authorize(Roles = "ADMIN")]
  public ResponseDto Delete(int id)
  {
    try
    {
      Product product = _db.Products.First(u => u.ProductId == id);
      if (!string.IsNullOrEmpty(product.ImageLocalPath))
      {
        var oldFilePathDirectory = Path.Combine(Directory.GetCurrentDirectory(), product.ImageLocalPath);
        FileInfo file = new(oldFilePathDirectory);
        if (file.Exists)
        {
          file.Delete();
        }
      }
      _db.Products.Remove(product);
      _db.SaveChanges();
    }
    catch (Exception ex)
    {
      _response.IsSuccess = false;
      _response.Message = ex.Message;
    }
    return _response;
  }
}
