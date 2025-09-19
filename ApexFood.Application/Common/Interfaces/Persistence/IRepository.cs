// ApexFood.Application/Common/Interfaces/Persistence/IRepository.cs
namespace ApexFood.Application.Common.Interfaces.Persistence;
public interface IRepository<T> where T : class
{
    Task AddAsync(T entity);
}