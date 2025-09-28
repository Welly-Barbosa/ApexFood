// Localização: src/ApexFood.Api/Controllers/InsumosController.cs
using Microsoft.AspNetCore.Mvc;
using ApexFood.Application.DTOs;
using ApexFood.Api.Services; // Adicione o using para o serviço

namespace ApexFood.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InsumosController : ControllerBase
{
    private readonly InsumoDataStore _insumoDataStore;

    // O serviço Singleton é injetado aqui pelo .NET
    public InsumosController(InsumoDataStore insumoDataStore) => _insumoDataStore = insumoDataStore;

    [HttpGet]
    public IActionResult GetInsumos([FromQuery] bool incluirInativos = false)
    {
        var insumos = _insumoDataStore.GetAll(incluirInativos);
        return Ok(insumos);
    }

    [HttpPost]
    public IActionResult CreateInsumo([FromBody] InsumoCreateDto novoInsumoDto)
    {
        if (novoInsumoDto == null)
        {
            return BadRequest("Dados do insumo não podem ser nulos.");
        }

        var insumoCriado = _insumoDataStore.Add(novoInsumoDto);

        return CreatedAtAction(nameof(GetInsumos), new { id = insumoCriado.Id }, insumoCriado);
    }
}