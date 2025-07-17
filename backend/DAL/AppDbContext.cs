using Microsoft.EntityFrameworkCore;
using Domain;
using Domain.Models;

namespace DAL
{
    public class AppDbContext : DbContext
    {
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Employer> Employers { get; set; }
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<OrganizationHoliday> OrganizationHolidays { get; set; }
        public DbSet<OrganizationWorkDay> OrganizationWorkDays { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<EmployeeInShift> EmployeeInShifts { get; set; }
        public DbSet<EmployeeInGroup> EmployeeInGroups { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<Shift> Shifts { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<OrganizationWorkDay> OrganizationWorkingHours { get; set; }
        public DbSet<WeekDay> WeekDays { get; set; }
        public DbSet<ShiftType> ShiftTypes { get; set; }
        
        //Employee preferences, sick leaves, vacations
        public DbSet<Vacation> Vacations { get; set; }
        public DbSet<VacationRequest> VacationRequests { get; set; }
        public DbSet<SickLeave> SickLeaves { get; set; }
        public DbSet<ShiftTypePreference> ShiftTypePreferences { get; set; }
        public DbSet<WeekDayPreference> WeekDayPreferences { get; set; }
        public DbSet<DayOffPreference> DayOffPreferences { get; set; }
        
        //Other Repository for admin panel
        public DbSet<SupportMessage> SupportMessages { get; set; }

        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<WeekDay>().HasData(
                new WeekDay { Id = 1, Name = "Monday" },
                new WeekDay { Id = 2, Name = "Tuesday" },
                new WeekDay { Id = 3, Name = "Wednesday" },
                new WeekDay { Id = 4, Name = "Thursday" },
                new WeekDay { Id = 5, Name = "Friday" },
                new WeekDay { Id = 6, Name = "Saturday" },
                new WeekDay { Id = 7, Name = "Sunday" }
            );
        }

        public AppDbContext(DbContextOptions<AppDbContext>? options = null)
            : base(options ?? new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql($"Host=localhost;Database=shift_ease;Username=postgres;Password=admin")
                .Options)
        {
        }
    }

}