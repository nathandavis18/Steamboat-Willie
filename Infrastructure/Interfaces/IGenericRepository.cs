using System.Linq.Expressions;

namespace Infrastructure.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        //Get object by ID
        T GetById(int? id);

        //Returns a set of objects osuing an expression filter (Essentially a WHERE clause in SQL)
        T Get(Expression<Func<T, bool>> predicate, bool trackChanges = false, string? includes = null);

        //Same as Get, but for Async calls
        Task<T> GetAsync(Expression<Func<T, bool>> predicate, bool trackChanges = false, string? includes = null);

        //Returns a list of results to iterate through
        IEnumerable<T> GetAll(Expression<Func<T, bool>>? predicate = null, Expression<Func<T, int>>? orderBy = null, string? includes = null);

        //Same as GetAll, but async
        Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? predicate = null, Expression<Func<T, int>>? orderBy = null, string? includes = null);

        //Add a new instance of the object
        void Add(T entity);

        //Delete a single instance of the object
        void Delete(T entity);

        //Delete a range of items in an object
        void Delete(IEnumerable<T> entities);

        //Update changes to an object
        void Update(T entity);
    }
}
