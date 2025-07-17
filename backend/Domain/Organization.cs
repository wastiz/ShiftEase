using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Models;

namespace Domain
{
    public class Organization
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; }

        public string? Description { get; set; } = null;
        public string? Address { get; set; } = null;
        public string? Phone { get; set; } = null;
        public string? Website { get; set; } = null;
        public string OrganizationType { get; set; }
        public bool IsOpen24_7 { get; set; } = false;
        public float NightShiftBonus { get; set; } = 0;
        public float HolidayBonus { get; set; } = 0;
        public string? PhotoUrl { get; set; } = null;
        public int EmployeeCount { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [Required]
        [ForeignKey("Employer")]
        public int EmployerId { get; set; }
        public Employer Employer { get; set; }
        public ICollection<OrganizationHoliday> OrganizationHolidays { get; set; }
        public ICollection<OrganizationWorkDay> OrganizationWorkDays { get; set; }
        public ICollection<Group> Groups { get; set; }
        public ICollection<Employee> Employees { get; set; }
        public ICollection<ShiftType> ShiftTypes { get; set; }
        public ICollection<Schedule> Schedules { get; set; }
    }
}