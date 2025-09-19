// ApexFood.Persistence/Repositories/InsumoRepository.cs
using ApexFood.Application.Common.Interfaces.Persistence;
using ApexFood.Domain.Entities;
using ApexFood.Persistence.Data;
namespace ApexFood.Persistence.Repositories;

public class InsumoRepository : Repository<Insumo>, IInsumoRepository
{
    public InsumoRepository(ApexFoodDbContext context) : base(context) { }
}