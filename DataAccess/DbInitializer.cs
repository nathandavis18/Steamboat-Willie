using Infrastructure.Interfaces;
using Infrastructure.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Utility;

namespace DataAccess
{
    public class DbInitializer : IDbInitializer
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DbInitializer(AppDbContext context, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        public void Initialize()
        {
            _context.Database.EnsureCreated();

            try
            {
                if (_context.Database.GetPendingMigrations().Any())
                {
                    _context.Database.Migrate();
                }
            }
            catch (Exception)
            {

            }

            if (_context.Departments.Any())
            {
                return;
            }

            var Departments = new List<Department>()
            {
                new Department { DepartmentName = "ACTG" },
                new Department { DepartmentName = "AERO" },
                new Department { DepartmentName = "ANTH" },
                new Department { DepartmentName = "ARCH" },
                new Department { DepartmentName = "ART"  },
                new Department { DepartmentName = "ARTH" },
                new Department { DepartmentName = "ASL"  },
                new Department { DepartmentName = "ASTR" },
                new Department { DepartmentName = "ATHL" },
                new Department { DepartmentName = "ATTC" },
                new Department { DepartmentName = "AUSV" },
                new Department { DepartmentName = "BIS"  },
                new Department { DepartmentName = "BME"  },
                new Department { DepartmentName = "BSAD" },
                new Department { DepartmentName = "BTNY" },
                new Department { DepartmentName = "CHEM" },
                new Department { DepartmentName = "CHF"  },
                new Department { DepartmentName = "CHNS" },
                new Department { DepartmentName = "CJ"   },
                new Department { DepartmentName = "CM"   },
                new Department { DepartmentName = "COMM" },
                new Department { DepartmentName = "CS"   },
                new Department { DepartmentName = "DANC" },
                new Department { DepartmentName = "DENT" },
                new Department { DepartmentName = "DMS"  },
                new Department { DepartmentName = "ECE"  },
                new Department { DepartmentName = "ECED" },
                new Department { DepartmentName = "ECON" },
                new Department { DepartmentName = "EDUC" },
                new Department { DepartmentName = "EEN"  },
                new Department { DepartmentName = "EET"  },
                new Department { DepartmentName = "ENGL" },
                new Department { DepartmentName = "ENGR" },
                new Department { DepartmentName = "ENTR" },
                new Department { DepartmentName = "ENVS" },
                new Department { DepartmentName = "ESS"  },
                new Department { DepartmentName = "ETC"  },
                new Department { DepartmentName = "ETM"  },
                new Department { DepartmentName = "FAM"  },
                new Department { DepartmentName = "FILM" },
                new Department { DepartmentName = "FIN"  },
                new Department { DepartmentName = "FL"   },
                new Department { DepartmentName = "FRCH" },
                new Department { DepartmentName = "FYE"  },
                new Department { DepartmentName = "GEO"  },
                new Department { DepartmentName = "GEOG" },
                new Department { DepartmentName = "GERT" },
                new Department { DepartmentName = "GRMN" },
                new Department { DepartmentName = "GSE"  },
                new Department { DepartmentName = "HAS"  },
                new Department { DepartmentName = "HIM"  },
                new Department { DepartmentName = "HIST" },
                new Department { DepartmentName = "HLTH" },
                new Department { DepartmentName = "HNRS" },
                new Department { DepartmentName = "HTHS" },
                new Department { DepartmentName = "HUMA" },
                new Department { DepartmentName = "IDT"  },
                new Department { DepartmentName = "ITLN" },
                new Department { DepartmentName = "JPNS" },
                new Department { DepartmentName = "KOR"  },
                new Department { DepartmentName = "LIBS" },
                new Department { DepartmentName = "LING" },
                new Department { DepartmentName = "MACC" },
                new Department { DepartmentName = "MATH" },
                new Department { DepartmentName = "MBA"  },
                new Department { DepartmentName = "MCJ"  },
                new Department { DepartmentName = "ME"   },
                new Department { DepartmentName = "MENG" },
                new Department { DepartmentName = "MET"  },
                new Department { DepartmentName = "MFET" },
                new Department { DepartmentName = "MGMT" },
                new Department { DepartmentName = "MHA"  },
                new Department { DepartmentName = "MICR" },
                new Department { DepartmentName = "MILS" },
                new Department { DepartmentName = "MIS"  },
                new Department { DepartmentName = "MKTG" },
                new Department { DepartmentName = "MLS"  },
                new Department { DepartmentName = "MPAS" },
                new Department { DepartmentName = "MPC"  },
                new Department { DepartmentName = "MSAT" },
                new Department { DepartmentName = "MSE"  },
                new Department { DepartmentName = "MSRS" },
                new Department { DepartmentName = "MSRT" },
                new Department { DepartmentName = "MSW"  },
                new Department { DepartmentName = "MTAX" },
                new Department { DepartmentName = "MTHE" },
                new Department { DepartmentName = "MUSC" },
                new Department { DepartmentName = "NAVS" },
                new Department { DepartmentName = "NET"  },
                new Department { DepartmentName = "NEUR" },
                new Department { DepartmentName = "NRSG" },
                new Department { DepartmentName = "NUCM" },
                new Department { DepartmentName = "NUTR" },
                new Department { DepartmentName = "OCRE" },
                new Department { DepartmentName = "OEHS" },
                new Department { DepartmentName = "PAR"  },
                new Department { DepartmentName = "PDD"  },
                new Department { DepartmentName = "PE"   },
                new Department { DepartmentName = "PEP"  },
                new Department { DepartmentName = "PHIL" },
                new Department { DepartmentName = "PHYS" },
                new Department { DepartmentName = "POLS" },
                new Department { DepartmentName = "PS"   },
                new Department { DepartmentName = "PSY"  },
                new Department { DepartmentName = "PTGS" },
                new Department { DepartmentName = "PUBH" },
                new Department { DepartmentName = "QS"   },
                new Department { DepartmentName = "QUAN" },
                new Department { DepartmentName = "RADT" },
                new Department { DepartmentName = "RATH" },
                new Department { DepartmentName = "REC"  },
                new Department { DepartmentName = "REST" },
                new Department { DepartmentName = "RGAF" },
                new Department { DepartmentName = "RHS"  },
                new Department { DepartmentName = "SBS"  },
                new Department { DepartmentName = "SCM"  },
                new Department { DepartmentName = "SE"   },
                new Department { DepartmentName = "SOC"  },
                new Department { DepartmentName = "SPAN" },
                new Department { DepartmentName = "SW"   },
                new Department { DepartmentName = "THEA" },
                new Department { DepartmentName = "UNIV" },
                new Department { DepartmentName = "WEB"  },
                new Department { DepartmentName = "WGS"  },
                new Department { DepartmentName = "WSU"  },
                new Department { DepartmentName = "ZOOL" }
            };

            foreach (var d in Departments)
            {
                _context.Departments.Add(d);
            }

            _roleManager.CreateAsync(new IdentityRole(SD.ADMIN_ROLE)).GetAwaiter().GetResult();
            _roleManager.CreateAsync(new IdentityRole(SD.CLIENT_ROLE)).GetAwaiter().GetResult();
            _roleManager.CreateAsync(new IdentityRole(SD.PROVIDER_ROLE)).GetAwaiter().GetResult();

            //Create a "Super" Admin Account
            _userManager.CreateAsync(new AppUser
            {
                UserName = "admin@gmail.com",
                Email = "admin@gmail.com",
                FName = "Admin",
                LName = "Test",
                PhoneNumber = "1234567890",
                DateOfBirth = DateTime.Parse("01/01/0001"),
                WNumber = "W00000000"
            }, "Pass1234!").GetAwaiter().GetResult();

            AppUser admin = _context.ApplicationUsers.FirstOrDefault(u => u.Email == "admin@gmail.com");
            _userManager.AddToRoleAsync(admin, SD.ADMIN_ROLE).GetAwaiter().GetResult();

            _userManager.CreateAsync(new AppUser
            {
                UserName = "client@gmail.com",
                Email = "client@gmail.com",
                FName = "Client",
                LName = "Test",
                PhoneNumber = "1234567890",
                DateOfBirth = DateTime.Parse("01/01/0001"),
                WNumber = "W00000000"
            }, "Pass1234!").GetAwaiter().GetResult();

            AppUser client = _context.ApplicationUsers.FirstOrDefault(u => u.Email == "client@gmail.com");
            _userManager.AddToRoleAsync(client, SD.CLIENT_ROLE).GetAwaiter().GetResult();

            Client c = new Client()
            {
                AppUserId = client.Id,
                DepartmentId = 2,
                ClassLevel = "Senior"
            };
            _context.Clients.Add(c);

            _userManager.CreateAsync(new AppUser
            {
                UserName = "provider@gmail.com",
                Email = "provider@gmail.com",
                FName = "Provider",
                LName = "Test",
                PhoneNumber = "1234567890",
                DateOfBirth = DateTime.Parse("01/01/0001"),
                WNumber = "W00000000"
            }, "Pass1234!").GetAwaiter().GetResult();

            AppUser provider = _context.ApplicationUsers.FirstOrDefault(u => u.Email == "provider@gmail.com");
            _userManager.AddToRoleAsync(provider, SD.PROVIDER_ROLE).GetAwaiter().GetResult();

            Provider p = new Provider()
            {
                AppUserId = provider.Id,
                DepartmentId = 2,
                Title = "Teacher"
            };

            _context.Providers.Add(p);

            _context.SaveChanges();
        }
    }
}
