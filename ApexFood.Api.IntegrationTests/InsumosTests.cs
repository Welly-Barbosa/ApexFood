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
        // 1. Preparar um ambiente limpo com um usuário autenticado
        using var scope = _scopeFactory.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var tokenGenerator = scope.ServiceProvider.GetRequiredService<IJwtTokenGenerator>();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApexFoodDbContext>();

        await dbContext.Database.MigrateAsync();

        var tenantId = IntegrationTestWebAppFactory.TestTenantId;
        //dbContext.Tenants.Add(tenant);
        //await dbContext.SaveChangesAsync();

        var user = new User { UserName = "user.insumo@test.com", Email = "user.insumo@test.com", TenantId = tenantId };
        await userManager.CreateAsync(user, "Password123!");

        var token = tokenGenerator.GenerateToken(user);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var requestDto = new CriarInsumoRequest("Farinha de Trigo Especial", "kg", "7891234567890", "FTE001");

        // Act
        // 2. Fazer a chamada POST para o novo endpoint
        var response = await _client.PostAsJsonAsync("/insumos", requestDto);

        // Assert
        // 3. Verificar a resposta da API
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdInsumo = await response.Content.ReadFromJsonAsync<object>();
        createdInsumo.Should().NotBeNull();

        // 4. Verificar diretamente no banco de dados se o insumo foi salvo corretamente
        var insumoSalvo = await dbContext.Insumos.FirstOrDefaultAsync(i => i.Nome == requestDto.Nome);
        insumoSalvo.Should().NotBeNull();
        insumoSalvo!.Nome.Should().Be(requestDto.Nome);
        insumoSalvo.Gtin.Should().Be(requestDto.Gtin);

        // A asserção mais importante: valida se o insumo foi associado ao tenant correto do usuário logado!
        insumoSalvo.TenantId.Should().Be(tenantId);
    }
}