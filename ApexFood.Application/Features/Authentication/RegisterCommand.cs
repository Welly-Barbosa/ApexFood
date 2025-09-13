// ApexFood.Application/Features/Authentication/RegisterCommand.cs

using ApexFood.Application.Common.Interfaces;
using ApexFood.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ApexFood.Application.Features.Authentication;

/// <summary>
/// Representa o comando para registrar um novo usuário.
/// </summary>
public record RegisterCommand(string Email, string Password) : IRequest<IdentityResult>;

/// <summary>
/// Manipulador para o comando de registro de usuário.
/// </summary>
public class RegisterCommandHandler : IRequestHandler<RegisterCommand, IdentityResult>
{
    private readonly UserManager<User> _userManager;
    private readonly IApplicationDbContext _context;

    public RegisterCommandHandler(UserManager<User> userManager, IApplicationDbContext context)

    {
        _userManager = userManager;
        _context = context;
    }

    public async Task<IdentityResult> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var defaultTenantId = new Guid("00000000-0000-0000-0000-000000000001");
        var tenantExists = await _context.Tenants.AnyAsync(t => t.Id == defaultTenantId, cancellationToken);
        if (!tenantExists)
        {
            throw new Exception("Tenant Matriz padrão não encontrado.");
        }

        var user = new User
        {
            UserName = request.Email,
            Email = request.Email,
            TenantId = defaultTenantId
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        return result;
    }
}