using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain;

public class PersonalDay
{
    [Key]
    public int Id { get; set; }

    [ForeignKey("Employee")]
    public int EmployeeId { get; set; }

    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
}
