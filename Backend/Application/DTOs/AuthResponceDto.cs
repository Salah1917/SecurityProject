using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs;

public class AuthResponseDto
{
    public string AccessToken { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
}