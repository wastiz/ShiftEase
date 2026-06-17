namespace Domain.Enums;

public static class NotificationTypes
{
    public const string ScheduleUpdate = "schedule_updated";
    
    public const string VacationRequest = "vacation_request";
    public const string VacationApproved = "vacation_approved";
    public const string VacationRejected = "vacation_rejected";
    
    public const string SickLeaveRequest = "sick_leave_request";
    public const string SickLeaveApproved = "sick_leave_approved";
    public const string SickLeaveRejected = "sick_leave_rejected";
    
    public const string PersonalDayRequest = "personal_day_request";
    public const string PersonalDayApproved = "personal_day_approved";
    public const string PersonalDayRejected = "personal_day_rejected";
    
    public const string Warning = "warning";
    public const string Error = "error";
}
