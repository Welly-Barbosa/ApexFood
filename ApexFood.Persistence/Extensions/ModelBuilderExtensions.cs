// ApexFood.Persistence/Extensions/ModelBuilderExtensions.cs
using ApexFood.Application.Common.Interfaces;
using ApexFood.Domain.Common;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection;

namespace ApexFood.Persistence.Extensions;

public static class ModelBuilderExtensions
{
    public static void AppendTenantQueryFilter(this ModelBuilder modelBuilder, ITenantResolver tenantResolver)
    {
        var tenantId = tenantResolver.GetTenantId();

        // Itera sobre todas as entidades mapeadas no DbContext
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // Verifica se a entidade implementa a interface ITenantEntity
            if (typeof(ITenantEntity).IsAssignableFrom(entityType.ClrType))
            {
                // Constrói a expressão de filtro: e => e.TenantId == tenantId
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var property = Expression.Property(parameter, nameof(ITenantEntity.TenantId));
                var constant = Expression.Constant(tenantId);
                var body = Expression.Equal(property, constant);
                var lambda = Expression.Lambda(body, parameter);

                // Aplica o filtro à entidade
                entityType.SetQueryFilter(lambda);
            }
        }
    }
}