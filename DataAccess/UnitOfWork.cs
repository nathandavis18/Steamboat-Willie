using Infrastructure.Interfaces;
using Infrastructure.Models;

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

        private IGenericRepository<Class> _Class;
        public IGenericRepository<Class> Class
        {
            get
            {
                if (_Class == null)
                {
                    _Class = new GenericRepository<Class>(_context);
                }
                return _Class;
            }
        }

        private IGenericRepository<ProviderClass> _ProviderClass;
        public IGenericRepository<ProviderClass> ProviderClass
        {
            get
            {
                if (_ProviderClass == null)
                {
                    _ProviderClass = new GenericRepository<ProviderClass>(_context);
                }
                return _ProviderClass;
            }
        }

        private IGenericRepository<GoogleToken> _GoogleToken;
        public IGenericRepository<GoogleToken> GoogleToken
        {
            get
            {
                if(_GoogleToken == null)
                {
                    _GoogleToken = new GenericRepository<GoogleToken>(_context);
                }
                return _GoogleToken;
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
