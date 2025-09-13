// ApexFood.Application/Features/Authentication/LoginQueryHandler.cs
using ApexFood.Application.Contracts.Authentication;
using ApexFood.Application.Common.Interfaces.Authentication; // Interface que criaremos a seguir
using ApexFood.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace ApexFood.Application.Features.Authentication;

public class LoginQueryHandler : IRequestHandler<LoginQuery, AuthenticationResponse>
{
    private readonly UserManager<User> _userManager;
    private readonly IJwtTokenGenerator _jwtTokenGenerator; // Dependência para gerar o token

    public LoginQueryHandler(UserManager<User> userManager, IJwtTokenGenerator jwtTokenGenerator)
    {
        _userManager = userManager;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<AuthenticationResponse> Handle(LoginQuery request, CancellationToken cancellationToken)
    {
        // 1. Verificar se o usuário existe
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            throw new Exception("Email ou senha inválidos."); // Lançamos uma exceção que será tratada pela API
        }

        // 2. Verificar se a senha está correta
        var isPasswordCorrect = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!isPasswordCorrect)
        {
            throw new Exception("Email ou senha inválidos.");
        }

        // 3. Gerar o token JWT
        var token = _jwtTokenGenerator.GenerateToken(user);

        // 4. Retornar a resposta com os dados do usuário e o token
        return new AuthenticationResponse(user.Id, user.Email!, token);
    }
}