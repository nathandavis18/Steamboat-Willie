using Infrastructure.Interfaces;
using Infrastructure.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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

            /*
              Seed Data for Department Table (DO LATER)
                 ACTG 
                 AERO 
                 ANTH 
                 ARCH 
                  ART 
                 ARTH 
                  ASL 
                 ASTR 
                 ATHL 
                 ATTC 
                 AUSV 
                  BIS 
                  BME 
                 BSAD 
                 BTNY 
                 CHEM 
                  CHF 
                 CHNS 
                 CJ 
                 CM 
                 COMM 
                 CS 
                 DANC 
                 DENT 
                  DMS 
                  ECE 
                 ECED 
                 ECON 
                 EDUC 
                  EEN 
                  EET 
                 ENGL 
                 ENGR 
                 ENTR 
                 ENVS 
                  ESS 
                  ETC 
                  ETM 
                  FAM 
                 FILM 
                  FIN 
                 FL 
                 FRCH 
                  FYE 
                  GEO 
                 GEOG 
                 GERT 
                 GRMN 
                  GSE 
                  HAS 
                  HIM 
                 HIST 
                 HLTH 
                 HNRS 
                 HTHS 
                 HUMA 
                  IDT 
                 ITLN 
                 JPNS 
                  KOR 
                 LIBS 
                 LING 
                 MACC 
                 MATH 
                  MBA 
                  MCJ 
                 ME 
                 MENG 
                  MET 
                 MFET 
                 MGMT 
                  MHA 
                 MICR 
                 MILS 
                  MIS 
                 MKTG 
                  MLS 
                 MPAS 
                  MPC 
                 MSAT 
                  MSE 
                 MSRS 
                 MSRT 
                  MSW 
                 MTAX 
                 MTHE 
                 MUSC 
                 NAVS 
                  NET 
                 NEUR 
                 NRSG 
                 NUCM 
                 NUTR 
                 OCRE 
                 OEHS 
                  PAR 
                  PDD 
                 PE 
                  PEP 
                 PHIL 
                 PHYS 
                 POLS 
                 PS 
                  PSY 
                 PTGS 
                 PUBH 
                 QS 
                 QUAN 
                 RADT 
                 RATH 
                  REC 
                 REST 
                 RGAF 
                  RHS 
                  SBS 
                  SCM 
                 SE 
                  SOC 
                 SPAN 
                 SW 
                 THEA 
                 UNIV 
                  WEB 
                  WGS 
                  WSU 
                 ZOOL 
            */
            _context.SaveChanges();
        }
    }
}
