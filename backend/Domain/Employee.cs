using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models
{
    public class Employee
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string LastName { get; set; }
        
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public string Password { get; set; }
        public string Phone { get; set; }
        public string Position { get; set; }
        public float Workload { get; set; }
        public float Salary { get; set; }
        public float SalaryInHour { get; set; }
        public string Priority { get; set; }
        
        public bool OnVacation { get; set; }
        public bool OnSickLeave { get; set; }
        public bool OnWork { get; set; }

        [ForeignKey("Group")]
        public int GroupId { get; set; }
        public Group Group { get; set; }
        
        [ForeignKey("Organization")]
        public int OrganizationId { get; set; }
        public Organization Organization { get; set; }
        
        public ICollection<EmployeeInShift> EmployeeShifts { get; set; }
    }
}