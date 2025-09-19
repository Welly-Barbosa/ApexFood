// ApexFood.Persistence/Repositories/Repository.cs
using ApexFood.Application.Common.Interfaces.Persistence;
using ApexFood.Persistence.Data;
namespace ApexFood.Persistence.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly ApexFoodDbContext _context;
    public Repository(ApexFoodDbContext context) => _context = context;
    public async Task AddAsync(T entity) => await _context.Set<T>().AddAsync(entity);
}