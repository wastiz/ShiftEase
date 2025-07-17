namespace Domain;
using System.ComponentModel.DataAnnotations;

public class WeekDay
{
    [Key] public int Id { get; set; }
    [Required] public string Name { get; set; }
}