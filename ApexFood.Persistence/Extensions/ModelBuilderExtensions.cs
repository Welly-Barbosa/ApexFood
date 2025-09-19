// ApexFood.Persistence/Extensions/ModelBuilderExtensions.cs
using ApexFood.Domain.Common;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ApexFood.Persistence.Extensions;

public static class ModelBuilderExtensions
{
    // ==================================================================
    // ASSINATURA CORRIGIDA: O método agora recebe a instância do DbContext.
    // ==================================================================
    public static void AppendTenantQueryFilter(this ModelBuilder modelBuilder, DbContext context)
    {
        // Itera sobre todas as entidades mapeadas no DbContext
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // Verifica se a entidade implementa a interface ITenantEntity
            if (typeof(ITenantEntity).IsAssignableFrom(entityType.ClrType))
            {
                // Constrói a expressão de filtro dinamicamente: 
                // e => EF.Property<Guid>(e, "TenantId") == context.TenantId
                var parameter = Expression.Parameter(entityType.ClrType, "e");

                var propertyMethodInfo = typeof(EF).GetMethod("Property")!.MakeGenericMethod(typeof(Guid));
                var propertyMethodCall = Expression.Call(null, propertyMethodInfo, parameter, Expression.Constant("TenantId"));

                // Acessa a propriedade TenantId da instância do DbContext passada como parâmetro
                var tenantIdProperty = Expression.Property(Expression.Constant(context), "TenantId");

                var body = Expression.Equal(propertyMethodCall, tenantIdProperty);
                var lambda = Expression.Lambda(body, parameter);

                // Aplica o filtro à entidade
                entityType.SetQueryFilter(lambda);
            }
        }
    }
}