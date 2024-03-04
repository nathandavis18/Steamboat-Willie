using Infrastructure.Interfaces;
using Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        private IGenericRepository<Appointment> _Appointment;
        public IGenericRepository<Appointment> Appointment
        {
            get
            {
                if(_Appointment == null)
                {
                    _Appointment = new GenericRepository<Appointment>(_context);
                }
                return _Appointment;
            }
        }
            
        private IGenericRepository<AppointmentCategory> _AppointmentCategory;
        public IGenericRepository<AppointmentCategory> AppointmentCategory
        {
            get
            {
                if(_AppointmentCategory == null)
                {
                    _AppointmentCategory = new GenericRepository<AppointmentCategory>(_context);
                }
                return _AppointmentCategory;
            }
        }

        private IGenericRepository<AppUser> _AppUser;
        public IGenericRepository<AppUser> AppUser
        {
            get
            {
                if(_AppUser == null)
                {
                    _AppUser = new GenericRepository<AppUser>(_context);
                }
                return _AppUser;
            }
        }

        private IGenericRepository<Client> _Client;
        public IGenericRepository<Client> Client
        {
            get
            {
                if(_Client == null) 
                { 
                    _Client = new GenericRepository<Client>(_context);
                }
                return _Client;
            }
        }

        private IGenericRepository<Department> _Department;
        public IGenericRepository<Department> Department
        {
            get
            {
                if(_Department == null)
                {
                    _Department = new GenericRepository<Department>(_context);
                }
                return _Department;
            }
        }

        private IGenericRepository<Location> _Location;
        public IGenericRepository<Location> Location
        {
            get
            {
                if(_Location == null) 
                {
                    _Location = new GenericRepository<Location>(_context);
                }
                return _Location;
            }
        }

        private IGenericRepository<Major> _Major;
        public IGenericRepository<Major> Major
        {
            get
            {
                if(_Major == null)
                {
                    _Major = new GenericRepository<Major>(_context);
                }
                return _Major;
            }
        }

        private IGenericRepository<Provider> _Provider;
        public IGenericRepository<Provider> Provider
        {
            get
            {
                if(_Provider == null)
                {
                    _Provider = new GenericRepository<Provider>(_context);
                }
                return _Provider;
            }
        }

        private IGenericRepository<ProviderAvailability> _ProviderAvailability;
        public IGenericRepository<ProviderAvailability> ProviderAvailability
        {
            get
            {
                if(_ProviderAvailability == null)
                {
                    _ProviderAvailability = new GenericRepository<ProviderAvailability>(_context);
                }
                return _ProviderAvailability;
            }
        }

        public int Commit()
        {
            return _context.SaveChanges();
        }

        public async Task<int> CommitAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
