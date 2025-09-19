// ApexFood.Persistence/Data/Configurations/TenantConfiguration.cs

using ApexFood.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApexFood.Persistence.Data.Configurations;

/// <summary>
/// Configuração da entidade Tenant para o Entity Framework Core usando a Fluent API.
/// </summary>
public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    /// <summary>
    /// Configura a entidade Tenant.
    /// </summary>
    /// <param name="builder">O construtor usado para configurar a entidade.</param>
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        // Define o nome da tabela no banco de dados.
        builder.ToTable("Tenants");

        // Define a chave primária da tabela.
        builder.HasKey(t => t.Id);

        // Configura a propriedade 'Name'.
        builder.Property(t => t.Name)
            .HasMaxLength(100) // Define um tamanho máximo para a coluna.
            .IsRequired(); // Marca a coluna como não-nula.

        // Configura a propriedade 'ParentTenantId' para ser opcional.
        builder.Property(t => t.ParentTenantId)
            .IsRequired(false);

        // Futuramente, poderíamos configurar a relação de auto-referência aqui:
        // builder.HasOne(t => t.ParentTenant)
        //     .WithMany(t => t.ChildTenants)
        //     .HasForeignKey(t => t.ParentTenantId)
        //     .OnDelete(DeleteBehavior.Restrict); // Evita deleção em cascata.
    }
}