using Infrastructure.Models;
namespace Infrastructure.Interfaces
{
    public interface IUnitOfWork
    {
        int Commit();
        Task<int> CommitAsync();
    }
}
