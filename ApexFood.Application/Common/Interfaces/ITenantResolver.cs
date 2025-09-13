// ApexFood.Application/Common/Interfaces/ITenantResolver.cs
namespace ApexFood.Application.Common.Interfaces;

public interface ITenantResolver
{
    Guid GetTenantId();
}