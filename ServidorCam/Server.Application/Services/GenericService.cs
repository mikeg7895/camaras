using Server.Application.Interfaces;
using Server.Core.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Server.Application.Services;

public abstract class GenericService<T>(IGenericRepository<T> repository) : IGenericService<T> where T : class
{
    protected readonly IGenericRepository<T> _repository = repository;

    public async Task<T> AddAsync(T valor)
    {
        return await _repository.AddAsync(valor);

    }

    public async Task<T?> GetByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _repository.GetAll().ToListAsync();
    }

    public void Update(T valor)
    {
        _repository.Update(valor);
    }

    public async Task<bool> Delete(int id)
    {
        await _repository.DeleteAsync(id);
        return true;
    }

    public async Task SaveChangesAsync()
    {
        await _repository.SaveChangesAsync();
    }
}