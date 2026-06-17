using System.Security.Cryptography;
using System.Text;

namespace BLL.Helpers;

public static class Helper
{
    
    public static string GenerateRandomPassword()
    {
        int length = 6;
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var data = new byte[length];
        var result = new StringBuilder(length);

        RandomNumberGenerator.Fill(data);

        foreach (var b in data)
            result.Append(chars[b % chars.Length]);

        return result.ToString();
    }
    
    public static DayOfWeek ParseDayOfWeek(string dayName)
    {
        return dayName.ToLower() switch
        {
            "monday" => DayOfWeek.Monday,
            "tuesday" => DayOfWeek.Tuesday,
            "wednesday" => DayOfWeek.Wednesday,
            "thursday" => DayOfWeek.Thursday,
            "friday" => DayOfWeek.Friday,
            "saturday" => DayOfWeek.Saturday,
            "sunday" => DayOfWeek.Sunday,
            _ => throw new ArgumentException($"Invalid day name: {dayName}")
        };
    }
}