using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS.Base.Domain.Interfaces.Repository
{
    public interface IGenericRepository<T> where T : class
    {
        Task AddAsync(T entity);
        Task<T?> GetByIdAsync(Guid id);
        Task<IEnumerable<T>> GetAllAsync();
    }
}
