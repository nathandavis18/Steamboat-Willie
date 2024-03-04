using Infrastructure.Models;
namespace Infrastructure.Interfaces
{
    public interface IUnitOfWork
    {
        IGenericRepository<Appointment> Appointment { get; }
        IGenericRepository<AppointmentCategory> AppointmentCategory { get; }
        IGenericRepository<AppUser> AppUser { get; }
        IGenericRepository<Client> Client { get; }
        IGenericRepository<Department> Department { get; }
        IGenericRepository<Location> Location { get; }
        IGenericRepository<Major> Major { get; }
        IGenericRepository<Provider> Provider { get; }
        IGenericRepository<ProviderAvailability> ProviderAvailability { get; }
        int Commit();
        Task<int> CommitAsync();
    }
}
