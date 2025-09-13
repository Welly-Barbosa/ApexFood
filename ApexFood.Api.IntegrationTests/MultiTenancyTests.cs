// ApexFood.Api.IntegrationTests/MultiTenancyTests.cs
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ApexFood.Application.Common.Interfaces.Authentication;
using ApexFood.Domain.Entities;
using ApexFood.Persistence.Data;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

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
        // 1. Preparar o banco de dados de teste com dados de dois tenants
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApexFoodDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<User>>();
        var tokenGenerator = scope.ServiceProvider.GetRequiredService<IJwtTokenGenerator>();

        await dbContext.Database.EnsureCreatedAsync(); // Garante que as tabelas existem

        var tenantA = new Tenant("Tenant A");
        var tenantB = new Tenant("Tenant B");

        var insumoA = new Insumo("Farinha de Trigo", tenantA.Id);
        var insumoB = new Insumo("Ovos", tenantB.Id);

        var userA = new User { UserName = "usera@test.com", Email = "usera@test.com", TenantId = tenantA.Id };
        await userManager.CreateAsync(userA, "Password123!");

        dbContext.Tenants.AddRange(tenantA, tenantB);
        dbContext.Insumos.AddRange(insumoA, insumoB);
        await dbContext.SaveChangesAsync();

        // 2. Gerar um token para o usuário do Tenant A
        var token = tokenGenerator.GenerateToken(userA);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        // 3. Fazer a requisição para o endpoint de insumos
        var response = await _client.GetAsync("/insumos");

        // Assert
        // 4. Verificar a resposta
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var insumos = await response.Content.ReadFromJsonAsync<List<Insumo>>();
        insumos.Should().NotBeNull();
        insumos.Should().ContainSingle(); // Deve conter apenas um insumo
        insumos!.First().Name.Should().Be(insumoA.Name); // E deve ser o insumo do Tenant A
        insumos!.First().TenantId.Should().Be(tenantA.Id);
    }
}