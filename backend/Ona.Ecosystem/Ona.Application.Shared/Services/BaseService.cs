using Ona.Core.Common.Exceptions;
using Ona.Domain.Shared.Entities;
using Ona.Domain.Shared.Interfaces;
using System.Linq.Expressions;

namespace Ona.Application.Shared.Services
{
    public class BaseService<TEntity> : IBaseService<TEntity> where TEntity : class
    {
        protected readonly IBaseRepository<TEntity> _repository;

        public BaseService(IBaseRepository<TEntity> repository)
        {
            _repository = repository;
        }

        public virtual async Task<TEntity> CreateAsync(TEntity entity)
        {
            if (entity is BaseEntity baseEntity)
            {
                baseEntity.Update();
            }

            var createdEntity = await _repository.CreateAsync(entity);
            await _repository.SaveChangesAsync();
            return createdEntity;
        }

        public virtual async Task<TEntity> UpdateAsync(TEntity entity)
        {
            if (entity is BaseEntity baseEntity)
            {
                baseEntity.Update();
            }

            var updatedEntity = _repository.Update(entity);
            await _repository.SaveChangesAsync();
            return updatedEntity;
        }

        public virtual async Task DeleteAsync(Guid id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
            {
                throw new NotFoundException($"{typeof(TEntity).Name} com id {id} nao encontrado.");
            }

            if (entity is BaseEntity baseEntity)
            {
                baseEntity.Delete();
                _repository.Update(entity);
            }
            else
            {
                _repository.Remove(entity);
            }

            await _repository.SaveChangesAsync();
        }

        public virtual async Task<TEntity?> GetByIdAsync(Guid id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public virtual async Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> filter)
        {
            return await _repository.FindAsync(filter);
        }

        public virtual async Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null)
        {
            return await _repository.CountAsync(predicate);
        }
    }
}
