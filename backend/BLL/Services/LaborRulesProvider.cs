using System.Text.Json;
using BLL.Contracts;
using BLL.Rules;
using Microsoft.AspNetCore.Hosting;

namespace BLL.Services;

public class LaborRulesProvider : ILaborRulesProvider
{
    private readonly LaborRules _rules;
    private readonly HashSet<DateTime> _holidays;

    public LaborRulesProvider(IWebHostEnvironment env)
    {
        var path = Path.Combine(env.ContentRootPath, "Rules", "EE", "laborRules.json");

        if (!File.Exists(path))
            throw new Exception($"Rules file not found: {path}");

        var json = File.ReadAllText(path);

        _rules = JsonSerializer.Deserialize<LaborRules>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? throw new Exception("Failed to deserialize labor rules");

        Validate(_rules);

        _holidays = GenerateHolidays(DateTime.UtcNow.Year);
    }

    public LaborRules GetLaborRules() => _rules;

    public MaxLimitsRules GetMaxLimitsRules() => _rules.MaxLimits;

    public RestPeriodsRules GetRestPeriodRules() => _rules.RestPeriods;
    
    public int GetMonthlyNormHours(int year, int month)
    {
        if (_rules.MonthlyNormHours != null &&
            _rules.MonthlyNormHours.TryGetValue(year, out var months) &&
            months.TryGetValue(month, out var hours))
        {
            return hours;
        }
        
        int workingDays = 0;
        int daysInMonth = DateTime.DaysInMonth(year, month);

        for (int day = 1; day <= daysInMonth; day++)
        {
            var date = new DateTime(year, month, day);

            if (IsWorkingDay(date))
                workingDays++;
        }

        return workingDays * _rules.StandardWork.HoursPerDay;
    }
    
    public bool IsWorkingDay(DateTime date)
    {
        if (date.DayOfWeek == DayOfWeek.Saturday ||
            date.DayOfWeek == DayOfWeek.Sunday)
            return false;

        if (IsHoliday(date))
            return false;

        return true;
    }
    
    public bool IsHoliday(DateTime date)
    {
        return _holidays.Contains(date.Date);
    }

    private HashSet<DateTime> GenerateHolidays(int year)
    {
        var holidays = new HashSet<DateTime>
        {
            new DateTime(year, 1, 1),   // New Year
            new DateTime(year, 2, 24),  // Independence Day
            new DateTime(year, 5, 1),   // Spring Day
            new DateTime(year, 6, 23),  // Victory Day
            new DateTime(year, 6, 24),  // Midsummer Day
            new DateTime(year, 8, 20),  // Restoration of Independence
            new DateTime(year, 12, 24), // Christmas Eve
            new DateTime(year, 12, 25), // Christmas
            new DateTime(year, 12, 26)  // Boxing Day
        };

        var easter = GetEasterSunday(year);

        holidays.Add(easter.AddDays(-2)); // Good Friday
        holidays.Add(easter);             // Easter Sunday
        holidays.Add(easter.AddDays(1));  // Easter Monday

        return holidays;
    }
    
    private DateTime GetEasterSunday(int year)
    {
        int a = year % 19;
        int b = year / 100;
        int c = year % 100;
        int d = b / 4;
        int e = b % 4;
        int f = (b + 8) / 25;
        int g = (b - f + 1) / 3;
        int h = (19 * a + b - d - g + 15) % 30;
        int i = c / 4;
        int k = c % 4;
        int l = (32 + 2 * e + 2 * i - h - k) % 7;
        int m = (a + 11 * h + 22 * l) / 451;

        int month = (h + l - 7 * m + 114) / 31;
        int day = ((h + l - 7 * m + 114) % 31) + 1;

        return new DateTime(year, month, day);
    }
    
    private void Validate(LaborRules rules)
    {
        if (rules.StandardWork.HoursPerDay <= 0)
            throw new Exception("Invalid HoursPerDay");

        if (rules.StandardWork.HoursPerWeek <= 0)
            throw new Exception("Invalid HoursPerWeek");

        if (rules.MaxLimits.MaxShiftLengthHours > 24)
            throw new Exception("Shift cannot exceed 24 hours");

        if (rules.RestPeriods.MinDailyRestHours < 11)
            throw new Exception("Daily rest must be at least 11 hours");
    }
}