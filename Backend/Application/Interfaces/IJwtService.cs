using System;
using System.Collections.Generic;
using System.Text;

using Domain.Entities;

namespace Application.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
}