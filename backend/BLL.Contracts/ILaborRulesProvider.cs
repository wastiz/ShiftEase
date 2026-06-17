using BLL.Rules;

namespace BLL.Contracts;

public interface ILaborRulesProvider
{
    LaborRules GetLaborRules();
    MaxLimitsRules GetMaxLimitsRules();
    RestPeriodsRules GetRestPeriodRules();
    int GetMonthlyNormHours(int year, int month);
    bool IsWorkingDay(DateTime date);
    bool IsHoliday(DateTime date);
}