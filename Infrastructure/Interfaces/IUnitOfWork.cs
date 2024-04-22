using Infrastructure.Models;
namespace Infrastructure.Interfaces
{
    public interface IUnitOfWork
    {
        IGenericRepository<Appointment> Appointment { get; }
        IGenericRepository<AppUser> AppUser { get; }
        IGenericRepository<Client> Client { get; }
        IGenericRepository<Department> Department { get; }
        IGenericRepository<Location> Location { get; }
        IGenericRepository<Provider> Provider { get; }
        IGenericRepository<ProviderAvailability> ProviderAvailability { get; }
        IGenericRepository<Class> Class { get; }
        IGenericRepository<ProviderClass> ProviderClass { get; }
        IGenericRepository<GoogleToken> GoogleToken {  get; }
        int Commit();
        Task<int> CommitAsync();
    }
}
