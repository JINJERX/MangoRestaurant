﻿namespace Mango.Services.CouponAPI.Models.Dtos;

public class ResponseDto
{
    public bool IsSuccess { get; set; } = true;
    public object Result { get; set; }
    public string DisplayMessage { get; set; } = String.Empty;
    public List<string> ErrorMessages { get; set; }
}