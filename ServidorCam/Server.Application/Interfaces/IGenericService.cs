namespace Server.Application.Interfaces;

public interface IGenericService<T>
{
    Task<T> AddAsync(T valor);
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    void Update(T valor);
    Task<bool> Delete(int id);
    Task SaveChangesAsync();
}