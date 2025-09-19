// ApexFood.Application/Contracts/Authentication/AuthenticationResponse.cs
namespace ApexFood.Application.Contracts.Authentication;

public record AuthenticationResponse(Guid Id, string Email, string Token);