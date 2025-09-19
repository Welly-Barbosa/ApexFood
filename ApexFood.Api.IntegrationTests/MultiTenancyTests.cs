// ApexFood.Api.IntegrationTests/MultiTenancyTests.cs
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ApexFood.Application.Common.Interfaces.Authentication;
using ApexFood.Domain.Entities;
using ApexFood.Persistence.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApexFoodDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var tokenGenerator = scope.ServiceProvider.GetRequiredService<IJwtTokenGenerator>();

        await dbContext.Database.MigrateAsync();

        // 1. Crie e SALVE os tenants PRIMEIRO.
        var tenantA = new Tenant("Tenant A");
        var tenantB = new Tenant("Tenant B");
        dbContext.Tenants.AddRange(tenantA, tenantB);
        await dbContext.SaveChangesAsync();

        // 2. Crie e SALVE o usuário, que depende de um tenant já existente.
        var userA = new User { UserName = "usera@test.com", Email = "usera@test.com", TenantId = tenantA.Id };
        var identityResult = await userManager.CreateAsync(userA, "Password123!");
        identityResult.Succeeded.Should().BeTrue();

        // ==================================================================
        // LINHAS CORRIGIDAS: Adicionado o parâmetro 'unidadeMedidaBase'.
        // ==================================================================
        var insumoA = new Insumo(tenantA.Id, "Farinha de Trigo", "kg");
        var insumoB = new Insumo(tenantB.Id, "Ovos", "un");

        dbContext.Insumos.AddRange(insumoA, insumoB);
        await dbContext.SaveChangesAsync();

        // 4. Gere o token para o usuário que agora existe no banco.
        var token = tokenGenerator.GenerateToken(userA);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/insumos");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // ATENÇÃO: A resposta da API será uma lista de Insumos. Precisamos de um DTO ou classe para desserializar.
        // Para este teste, vamos usar um tipo anônimo ou um record simples.
        var insumos = await response.Content.ReadFromJsonAsync<List<InsumoTestResponse>>();

        insumos.Should().NotBeNull();
        insumos.Should().ContainSingle();
        insumos!.First().Nome.Should().Be(insumoA.Nome);
        insumos!.First().TenantId.Should().Be(tenantA.Id);
    }

    // Classe auxiliar para desserialização da resposta JSON no teste.
    private record InsumoTestResponse(Guid Id, Guid TenantId, string Nome, string UnidadeMedidaBase, string? Gtin, string? Sku, bool IsAtivo);
}