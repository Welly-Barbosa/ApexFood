// ApexFood.Application/Features/Authentication/LoginQuery.cs
using MediatR;
using ApexFood.Application.Contracts.Authentication;

namespace ApexFood.Application.Features.Authentication;

// Esta query recebe os dados de login e espera uma resposta de autenticação (com o token).
public record LoginQuery(string Email, string Password) : IRequest<AuthenticationResponse>;