import {DateData, EmployeeTimeOff, Holiday, TimeOffType, WorkDay} from "@/types";

export const MONTHS = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec",];
export const MONTH_NAMES: Record<string, number> = {
    january: 1, february: 2, march: 3, april: 4, may: 5, june: 6,
    july: 7, august: 8, september: 9, october: 10, november: 11, december: 12,
};

export const WEEK_DAYS = [
    { value: "Monday", label: "Mon" },
    { value: "Tuesday", label: "Tue" },
    { value: "Wednesday", label: "Wed" },
    { value: "Thursday", label: "Thu" },
    { value: "Friday", label: "Fri" },
    { value: "Saturday", label: "Sat" },
    { value: "Sunday", label: "Sun" },
]

export function getDaysInMonth(year: number, month: number): DateData[] {
    return Array.from(
        { length: new Date(year, month + 1, 0).getDate() },
        (_, i) => {
            const date = new Date(year, month, i + 1);

            const y = date.getFullYear();
            const m = (date.getMonth() + 1).toString().padStart(2, '0');
            const d = date.getDate().toString().padStart(2, '0');

            const isoDate = `${y}-${m}-${d}`;

            const weekday = date.toLocaleDateString('en-US', {
                weekday: 'short',
            });

            const label = `${d}.${m} ${weekday}`;

            return { isoDate, label };
        }
    );
}

export function getPreviousDay(dateStr: string): string {
    const date = new Date(dateStr);
    date.setDate(date.getDate() - 1);
    return date.toISOString().split('T')[0];
}

export function timeToMinutes(time: string): number {
    const [hours, minutes] = time.split(':').map(Number);
    return hours * 60 + minutes;
}


export function isHoliday(date: string, holidays: Holiday[]) {
    const d = new Date(date + 'T00:00:00')
    return holidays.some(
        h =>
            h.month === d.getMonth() + 1 &&
            h.day === d.getDate()
    )
}

export const dayNames = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday']

export function isWorkingDay(date: string, workDays: WorkDay[]) {
    const d = new Date(date + 'T00:00:00')
    const dayOfWeek = d.getDay()
    const currentDayName = dayNames[dayOfWeek]

    return workDays.some(wd => wd.dayOfWeek === currentDayName)
}

export function getHolidayName(date: string, holidays: Holiday[]) {
    const d = new Date(date)
    const holiday = holidays.find(h => h.month === d.getUTCMonth() + 1 && h.day === d.getUTCDate())
    return holiday?.holidayName || 'Holiday'
}

export function getEmployeeTimeOff(employeeId: number, date: string, timeOffs: EmployeeTimeOff[]): EmployeeTimeOff | undefined {
    const dateObj = new Date(date)
    return timeOffs.find(timeOff => {
        if (timeOff.employeeId !== employeeId) return false
        const startDate = new Date(timeOff.startDate)
        const endDate = new Date(timeOff.endDate)
        return dateObj >= startDate && dateObj <= endDate
    })
}

export function getTimeOffLabelKey(type: TimeOffType): string {
    switch (type) {
        case TimeOffType.Vacation:
            return 'timeOffVacation'
        case TimeOffType.SickLeave:
            return 'timeOffSickLeave'
        case TimeOffType.PersonalDay:
            return 'timeOffPersonalDay'
        default:
            return 'timeOffDayOff'
    }
}

export function getTimeOffColor(type: TimeOffType): string {
    switch (type) {
        case TimeOffType.Vacation:
            return 'bg-blue-100 dark:bg-blue-950 border-blue-300 dark:border-blue-700'
        case TimeOffType.SickLeave:
            return 'bg-orange-100 dark:bg-orange-950 border-orange-300 dark:border-orange-700'
        case TimeOffType.PersonalDay:
            return 'bg-purple-100 dark:bg-purple-950 border-purple-300 dark:border-purple-700'
        default:
            return 'bg-gray-100 dark:bg-gray-950 border-gray-300 dark:border-gray-700'
    }
}
export function roundToMinutes(time: string): string {
    const [hours, minutes] = time.split(':');
    return `${hours}:${minutes}`;
}

