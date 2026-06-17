using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models
{
    public class Employee
    {
        [Key] public int Id { get; set; }
        [Required] [MaxLength(100)] public string FirstName { get; set; }
        [Required] [MaxLength(100)] public string LastName { get; set; }
        [Required] [EmailAddress] public string Email { get; set; }
        [Required] public string Password { get; set; }
        public string Phone { get; set; }
        [Required] public string Position { get; set; }
        public float? HourlyRate { get; set; }
        [Range(0.0, 1.0)] public float EmploymentRate { get; set; } = 1.0f;
        public string? Priority { get; set; } = "Low";
        public bool OnVacation { get; set; }
        public bool OnSickLeave { get; set; }
        public bool OnWork { get; set; }

        [ForeignKey("Organization")]
        public int OrganizationId { get; set; }
        public Organization Organization { get; set; }

        public ICollection<EmployeeInShift> EmployeeShifts { get; set; }
        public ICollection<EmployeeInDepartment> EmployeeDepartments { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
