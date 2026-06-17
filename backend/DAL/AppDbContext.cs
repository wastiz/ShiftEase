using System.Globalization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Domain;
using Domain.Enums;
using Domain.Models;

namespace DAL
{
    public class AppDbContext : DbContext, IDataProtectionKeyContext
    {
        private readonly IDataProtector? _protector;

        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
        public DbSet<Employer> Employers { get; set; }
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<OrganizationHoliday> OrganizationHolidays { get; set; }
        public DbSet<OrganizationWorkDay> OrganizationWorkDays { get; set; }
        public DbSet<EmployeeInShift> EmployeeInShifts { get; set; }
        public DbSet<EmployeeInDepartment> EmployeeInDepartments { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<Shift> Shifts { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<OrganizationWorkDay> OrganizationWorkingHours { get; set; }
        public DbSet<ShiftTemplate> ShiftTypes { get; set; }
        public DbSet<Employee> Employees { get; set; }

        public DbSet<EmployeeMonthlyHours> EmployeeMonthlyHours { get; set; }
        public DbSet<Vacation> Vacations { get; set; }
        public DbSet<VacationRequest> VacationRequests { get; set; }
        public DbSet<SickLeave> SickLeaves { get; set; }
        public DbSet<SickLeaveRequest> SickLeaveRequests { get; set; }
        public DbSet<PersonalDay> PersonalDays { get; set; }
        public DbSet<PersonalDayRequest> PersonalDayRequests { get; set; }
        public DbSet<ShiftTypePreference> ShiftTypePreferences { get; set; }
        public DbSet<WeekDayPreference> WeekDayPreferences { get; set; }

        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasPostgresEnum<DayOfWeek>();
            modelBuilder.HasPostgresEnum<UserRole>();
            modelBuilder.HasPostgresEnum<SchedulePattern>();

            // HourlyRate must be stored as text to support encryption
            modelBuilder.Entity<Employee>()
                .Property(e => e.HourlyRate)
                .HasColumnType("text");

            if (_protector == null) return;

            var protector = _protector;

            var stringConverter = new ValueConverter<string?, string?>(
                v => v == null ? null : protector.Protect(v),
                v => DecryptString(v, protector)
            );

            var floatConverter = new ValueConverter<float?, string?>(
                v => v == null ? null : protector.Protect(v.Value.ToString(CultureInfo.InvariantCulture)),
                v => DecryptFloat(v, protector)
            );

            modelBuilder.Entity<Employee>().Property(e => e.Phone).HasConversion(stringConverter);
            modelBuilder.Entity<Employer>().Property(e => e.Phone).HasConversion(stringConverter);
            modelBuilder.Entity<Employee>().Property(e => e.HourlyRate).HasConversion(floatConverter);
        }

        public AppDbContext(DbContextOptions<AppDbContext> options, IDataProtectionProvider? dataProtectionProvider = null)
            : base(options)
        {
            _protector = dataProtectionProvider?.CreateProtector("ShiftEase.PersonalData");
        }

        private static string? DecryptString(string? value, IDataProtector protector)
        {
            if (value == null) return null;
            try { return protector.Unprotect(value); }
            catch { return value; } // plain text fallback for pre-existing unencrypted values
        }

        private static float? DecryptFloat(string? value, IDataProtector protector)
        {
            if (value == null) return null;
            try { return float.Parse(protector.Unprotect(value), CultureInfo.InvariantCulture); }
            catch { return float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var plain) ? plain : null; }
        }
    }
}
