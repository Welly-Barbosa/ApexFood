// ApexFood.Api.IntegrationTests/MultiTenancyTests.cs
using ApexFood.Application.Common.Interfaces.Authentication;
using ApexFood.Domain.Entities;
using ApexFood.Persistence.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace ApexFood.Api.IntegrationTests;

public class MultiTenancyTests : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly HttpClient _client;
    private readonly IServiceScopeFactory _scopeFactory;

    public MultiTenancyTests(IntegrationTestWebAppFactory factory)
    {
        _client = factory.CreateClient();
        _scopeFactory = factory.Services.GetRequiredService<IServiceScopeFactory>();
    }

    [Fact]
    public async Task GetInsumos_WhenAuthenticatedAsTenantA_ShouldOnlyReturnTenantAData()
    {
        // Arrange
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApexFoodDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var tokenGenerator = scope.ServiceProvider.GetRequiredService<IJwtTokenGenerator>();

        await dbContext.Database.MigrateAsync();

        // 1. Crie e salve os tenants
        var tenantA = new Tenant("Tenant A", IntegrationTestWebAppFactory.TestTenantId);
        var tenantB = new Tenant("Tenant B", Guid.NewGuid());
        dbContext.Tenants.AddRange(tenantA, tenantB);
        await dbContext.SaveChangesAsync();

        // 2. Crie e salve o usuário associado ao Tenant A
        var userA = new User { UserName = "usera@test.com", Email = "usera@test.com", TenantId = tenantA.Id };
        var identityResult = await userManager.CreateAsync(userA, "Password123!");
        identityResult.Succeeded.Should().BeTrue();

        // ==================================================================
        // PASSO CORRIGIDO: Usando o construtor correto da entidade Insumo
        // ==================================================================
        var insumoA = new Insumo(tenantA.Id, "Farinha de Trigo", "kg", null, null);
        var insumoB = new Insumo(tenantB.Id, "Ovos", "un", null, null);
        dbContext.Insumos.AddRange(insumoA, insumoB);
        await dbContext.SaveChangesAsync();

        // 4. Gere o token para o usuário do Tenant A
        var token = tokenGenerator.GenerateToken(userA);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/insumos");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var insumos = await response.Content.ReadFromJsonAsync<List<Insumo>>();
        insumos.Should().NotBeNull();
        insumos.Should().ContainSingle();

        // ==================================================================
        // PASSO CORRIGIDO: Acessando a propriedade correta 'Nome'
        // ==================================================================
        insumos!.First().Nome.Should().Be(insumoA.Nome);
        insumos!.First().TenantId.Should().Be(tenantA.Id);
    }
}