// ApexFood.Application/Features/Insumos/CriarInsumoCommandHandler.cs
using ApexFood.Application.Common.Interfaces;
using ApexFood.Application.Common.Interfaces.Persistence;
using ApexFood.Domain.Entities;
using MediatR;
namespace ApexFood.Application.Features.Insumos;

public class CriarInsumoCommandHandler : IRequestHandler<CriarInsumoCommand, Guid>
{
    private readonly IInsumoRepository _insumoRepository;
    private readonly IApplicationDbContext _context;
    private readonly ITenantResolver _tenantResolver;

    public CriarInsumoCommandHandler(IInsumoRepository insumoRepository, IApplicationDbContext context, ITenantResolver tenantResolver)
    {
        _insumoRepository = insumoRepository;
        _context = context;
        _tenantResolver = tenantResolver;
    }

    public async Task<Guid> Handle(CriarInsumoCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantResolver.GetTenantId();
        var insumo = new Insumo(
            tenantId,
            request.Nome,
            request.UnidadeMedidaBase,
            request.Gtin,
            request.Sku);

        await _insumoRepository.AddAsync(insumo);
        await _context.SaveChangesAsync(cancellationToken);

        return insumo.Id;
    }
}