// ApexFood.Infrastructure/Services/TenantResolver.cs
using ApexFood.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace ApexFood.Infrastructure.Services;

public class TenantResolver : ITenantResolver
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantResolver(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid GetTenantId()
    {
        // Se não houver contexto HTTP (ex: durante a inicialização), retorne um Guid vazio.
        // O filtro funcionará, mas não filtrará nada, o que é seguro para a construção do modelo.
        var tenantIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue("tenantId");

        return tenantIdClaim is not null ? Guid.Parse(tenantIdClaim) : Guid.Empty;
    }
}