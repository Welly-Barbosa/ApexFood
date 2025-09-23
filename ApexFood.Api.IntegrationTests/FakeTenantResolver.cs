// ApexFood.Api.IntegrationTests/FakeTenantResolver.cs
using ApexFood.Application.Common.Interfaces;

namespace ApexFood.Api.IntegrationTests;

/// <summary>
/// Uma implementação "fake" do ITenantResolver para ser usada exclusivamente
/// em testes de integração. Ela sempre retorna um Tenant ID fixo e conhecido.
/// </summary>
public class FakeTenantResolver : ITenantResolver
{
    private readonly Guid _tenantId;

    public FakeTenantResolver(Guid tenantId)
    {
        _tenantId = tenantId;
    }

    public Guid GetTenantId() => _tenantId;
}