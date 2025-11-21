using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Core.Domain.Interfaces;

public interface IGenericRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    IQueryable<T> GetAll();
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    void Update(T entity);
    Task DeleteAsync(int id);
    Task SaveChangesAsync();
}
