using AccountApi.Data;
using AccountApi.Repo.IRepository;
using Microsoft.EntityFrameworkCore;

namespace AccountApi.Repo.Repository;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected AppDbContext _context;
    internal DbSet<T> dbSet;
    protected readonly ILogger _logger;

    public GenericRepository(AppDbContext context, ILogger logger)
    {
        _context = context;
        _logger = logger;
        dbSet = context.Set<T>();
    }

    public virtual async Task<bool> Add(T entity)
    {
        try
        {
            await dbSet.AddAsync(entity);
            await _context.SaveChangesAsync(); 

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "{Repo}, Add에서 에러가 발생하였습니다.", typeof(GenericRepository<T>));
            return false;
        }
       
    }

    public virtual async Task<IEnumerable<T>> All()
    {
        return await dbSet.ToListAsync();
    }

    public virtual Task<bool> Delete(object id)
    {
        throw new NotImplementedException();
    }

    public virtual async Task<T> GetById(object id)
    {
        return await dbSet.FindAsync(id);
    }

    public virtual async Task<bool> Update(T entity)
    {
        try
        {
            dbSet.Update(entity);
            await _context.SaveChangesAsync(); 
            
            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "{Repo}, Update에서 에러가 발생하였습니다.", typeof(GenericRepository<T>));
            return false;
        }
    }

}
