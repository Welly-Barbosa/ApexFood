// ApexFood.Api.IntegrationTests/MultiTenancyTests.cs
using ApexFood.Application.Common.Interfaces.Authentication;
using ApexFood.Domain.Entities;
using ApexFood.Persistence.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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

        // ==================================================================
        // LÓGICA DE SEEDING CORRIGIDA
        // ==================================================================

        // 1. Crie e SALVE os tenants PRIMEIRO.
        var tenantA = new Tenant("Tenant A");
        var tenantB = new Tenant("Tenant B");
        dbContext.Tenants.AddRange(tenantA, tenantB);
        await dbContext.SaveChangesAsync();

        // 2. Crie e SALVE o usuário, que depende de um tenant já existente.
        var userA = new User { UserName = "usera@test.com", Email = "usera@test.com", TenantId = tenantA.Id };
        var identityResult = await userManager.CreateAsync(userA, "Password123!");
        identityResult.Succeeded.Should().BeTrue(); // Adiciona uma verificação de que o usuário foi criado

        // 3. Crie e SALVE os insumos, que dependem dos tenants.
        var insumoA = new Insumo("Farinha de Trigo", tenantA.Id);
        var insumoB = new Insumo("Ovos", tenantB.Id);
        dbContext.Insumos.AddRange(insumoA, insumoB);
        await dbContext.SaveChangesAsync();

        // 4. Gere o token para o usuário que agora existe no banco.
        var token = tokenGenerator.GenerateToken(userA);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/insumos");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var insumos = await response.Content.ReadFromJsonAsync<List<Insumo>>();
        insumos.Should().NotBeNull();
        insumos.Should().ContainSingle();
        insumos!.First().Name.Should().Be(insumoA.Name);
        insumos!.First().TenantId.Should().Be(tenantA.Id);
    }
}