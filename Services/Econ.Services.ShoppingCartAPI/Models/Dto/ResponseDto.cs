﻿namespace Econ.Services.ShoppingCartAPI;

public class ResponseDto
{
  public object? Result { get; set; }
  public bool IsSuccess { get; set; } = true;
  public string Message { get; set; } = "";
}

