namespace Domain;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class WeekDayPreference
{
    [Key] public int Id { get; set; }
    [ForeignKey("Employee")] public int EmployeeId { get; set; }
    [ForeignKey("WeekDay")] public int WeekDayId { get; set; }
    public WeekDay WeekDay { get; set; }
}