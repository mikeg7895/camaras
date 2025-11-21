using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Server.Core.Domain.Interfaces;
using Server.Infrastructure.Persistence;

namespace Server.Infrastructure.Repositories;

public abstract class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly ServerDbContext _context;
    protected DbSet<T> Entities => _context.Set<T>();

    protected GenericRepository(ServerDbContext context)
    {
        _context = context;
    }

    public virtual async Task<T> AddAsync(T valor)
    {
        await Entities.AddAsync(valor);
        return valor;
    }

    public virtual async Task<T?> GetByIdAsync(int id)  => await Entities.FindAsync(id);

    public virtual IQueryable<T> GetAll() => Entities;

    public virtual async Task<IEnumerable<T>> GetAllAsync() => await Entities.ToListAsync();

    public virtual void Update(T valor)
    {
        Entities.Update(valor);
    }

    public virtual async Task DeleteAsync(int id)
    {
        var entidad = await GetByIdAsync(id);
        if (entidad == null) return;

        Entities.Remove(entidad);
    }

    public virtual async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
