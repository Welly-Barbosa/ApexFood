// ApexFood.Application/Common/Interfaces/Authentication/IJwtTokenGenerator.cs
using ApexFood.Domain.Entities;

namespace ApexFood.Application.Common.Interfaces.Authentication;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}