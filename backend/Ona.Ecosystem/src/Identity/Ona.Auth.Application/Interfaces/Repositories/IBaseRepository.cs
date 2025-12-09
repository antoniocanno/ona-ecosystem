using System.Linq.Expressions;

namespace Ona.Auth.Application.Interfaces.Repositories
{
    public interface IBaseRepository<TEntity> where TEntity : class
    {
        Task<TEntity> CreateAsync(TEntity entity);
        TEntity Update(TEntity entity);
        void Remove(TEntity entity);
        Task<TEntity?> GetByIdAsync(Guid id);
        Task<IEnumerable<TEntity>> GetAllAsync();
        Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> filter);
        Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null);
        Task SaveChangesAsync();
    }
}
