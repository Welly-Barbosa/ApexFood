// ApexFood.Api.IntegrationTests/InsumosTests.cs
using ApexFood.Api.Contracts.Insumos;
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

public class InsumosTests : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly HttpClient _client;
    private readonly IServiceScopeFactory _scopeFactory;

    public InsumosTests(IntegrationTestWebAppFactory factory)
    {
        _client = factory.CreateClient();
        _scopeFactory = factory.Services.GetRequiredService<IServiceScopeFactory>();
    }

    [Fact]
    public async Task CriarInsumo_ComDadosValidos_DeveRetornar201CreatedESalvarNoBanco()
    {
        // Arrange
        using var scope = _scopeFactory.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var tokenGenerator = scope.ServiceProvider.GetRequiredService<IJwtTokenGenerator>();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApexFoodDbContext>();

        await dbContext.Database.MigrateAsync();

        // ==================================================================
        // LÓGICA DE SEEDING CORRIGIDA (A CAUSA RAIZ)
        // ==================================================================

        // 1. Defina o TenantId e CRIE E SALVE o Tenant PRIMEIRO.
        var tenantId = IntegrationTestWebAppFactory.TestTenantId;
        var tenant = new Tenant("Tenant de Teste para Insumos", tenantId); // Usando um construtor apropriado
        dbContext.Tenants.Add(tenant);
        await dbContext.SaveChangesAsync();

        // 2. Agora, com o Tenant já existindo no banco, crie e salve o usuário.
        var user = new User { UserName = "user.insumo@test.com", Email = "user.insumo@test.com", TenantId = tenantId };
        var identityResult = await userManager.CreateAsync(user, "Password123!");
        identityResult.Succeeded.Should().BeTrue();

        // 3. Gere o token para o usuário autenticado.
        var token = tokenGenerator.GenerateToken(user);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var requestDto = new CriarInsumoRequest("Farinha de Trigo Especial", "kg", "7891234567890", "FTE001");

        // Act
        var response = await _client.PostAsJsonAsync("/insumos", requestDto);

        // Assert
        // 4. Verifique a resposta da API.
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        // 5. Verifique diretamente no banco de dados se o insumo foi salvo corretamente.
        var insumoSalvo = await dbContext.Insumos
            .FirstOrDefaultAsync(i => i.Nome == requestDto.Nome);

        insumoSalvo.Should().NotBeNull();
        insumoSalvo!.Nome.Should().Be(requestDto.Nome);
        insumoSalvo.Gtin.Should().Be(requestDto.Gtin);
        insumoSalvo.TenantId.Should().Be(tenantId);
    }
}