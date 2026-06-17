using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices.JavaScript;

namespace Domain.Models
{
    public class Shift
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateOnly Date { get; set; }

        [ForeignKey("ShiftTemplate")]
        public int ShiftTemplateId { get; set; }
        public ShiftTemplate ShiftTemplate { get; set; }

        [ForeignKey("Schedule")]
        public int ScheduleId { get; set; }
        public Schedule Schedule { get; set; }

        public List<EmployeeInShift> EmployeeInShifts { get; set; } = new();
    }
}
