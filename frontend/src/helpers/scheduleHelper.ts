import { Shift, EmployeeMinData, ShiftTemplate } from "@/types";

export type EmployeeScheduleRow = {
    employeeId: number;
    employeeName: string;
    position: string;
    departmentNames: string[];
    totalHours: number;
    days: Map<string, DayAssignment>;
};

export type DayAssignment = {
    shiftId: number;
    shiftTypeId: number;
    shiftTypeName: string;
    startTime: string;
    endTime: string;
    color: string;
    note?: string;
};

export function transformToSimpleView(
    employees: EmployeeMinData[],
    shifts: Shift[],
    daysOfMonth: string[],
    shiftTypes?: ShiftTemplate[]
): EmployeeScheduleRow[] {
    return employees.map((emp) => {
        const employeeShifts = shifts.filter((shift) =>
            shift.employees.some((e) => e.id === emp.id)
        );

        const daysMap = new Map<string, DayAssignment>();

        employeeShifts.forEach((shift) => {
            const employeeAssignment = shift.employees.find((e) => e.id === emp.id);
            daysMap.set(shift.date, {
                shiftId: shift.id,
                shiftTypeId: shift.shiftTypeId,
                shiftTypeName: shift.shiftTypeName,
                startTime: shift.startTime,
                endTime: shift.endTime,
                color: shift.color,
                note: employeeAssignment?.note,
            });
        });

        const totalHours = employeeShifts.reduce((sum, shift) => {
            const shiftType = shiftTypes?.find((st) => st.id === shift.shiftTypeId);
            const duration = calculateShiftDuration(shift.startTime, shift.endTime, shiftType?.breakDuration);
            return sum + duration;
        }, 0);

        return {
            employeeId: emp.id,
            employeeName: emp.name,
            position: emp.departmentNames.join(', '),
            departmentNames: emp.departmentNames,
            totalHours,
            days: daysMap,
        };
    });
}

export function transformFromSimpleView(
    rows: EmployeeScheduleRow[],
    originalShifts: Shift[]
): Shift[] {
    const shiftsMap = new Map<string, Shift>();

    originalShifts.forEach((shift) => {
        const key = `${shift.date}-${shift.shiftTypeId}`;
        shiftsMap.set(key, { ...shift, employees: [] });
    });

    rows.forEach((row) => {
        row.days.forEach((assignment, date) => {
            const key = `${date}-${assignment.shiftTypeId}`;
            let shift = shiftsMap.get(key);

            if (!shift) {
                shift = {
                    id: assignment.shiftId,
                    date,
                    shiftTypeId: assignment.shiftTypeId,
                    shiftTypeName: assignment.shiftTypeName,
                    startTime: assignment.startTime,
                    endTime: assignment.endTime,
                    color: assignment.color,
                    employeeNeeded: 1,
                    employees: [],
                };
                shiftsMap.set(key, shift);
            }

            shift.employees.push({
                id: row.employeeId,
                name: row.employeeName,
                note: assignment.note
            });
        });
    });

    return Array.from(shiftsMap.values()).filter((shift) => shift.employees.length > 0);
}

function calculateShiftDuration(startTime: string, endTime: string, breakDuration?: string | null): number {
    const [startH, startM] = startTime.split(":").map(Number);
    const [endH, endM] = endTime.split(":").map(Number);

    let duration = (endH * 60 + endM) - (startH * 60 + startM);
    if (duration < 0) duration += 24 * 60;

    if (breakDuration) {
        const breakMinutes = parseBreakToMinutes(breakDuration);
        duration = Math.max(0, duration - breakMinutes);
    }

    return duration / 60;
}

export function parseBreakToMinutes(breakDuration: string): number {
    const parts = breakDuration.split(':').map(Number);
    return (parts[0] || 0) * 60 + (parts[1] || 0);
}

export function minutesToTimeSpan(minutes: number): string {
    const h = Math.floor(minutes / 60);
    const m = minutes % 60;
    return `${String(h).padStart(2, '0')}:${String(m).padStart(2, '0')}:00`;
}

export function getWorkingHoursLabel(startTime: string, endTime: string, breakDuration?: string | null): string {
    const [startH, startM] = startTime.split(":").map(Number);
    const [endH, endM] = endTime.split(":").map(Number);

    let durationMin = (endH * 60 + endM) - (startH * 60 + startM);
    if (durationMin < 0) durationMin += 24 * 60;

    if (breakDuration) {
        durationMin = Math.max(0, durationMin - parseBreakToMinutes(breakDuration));
    }

    const h = Math.floor(durationMin / 60);
    const m = durationMin % 60;
    return m > 0 ? `${h}h ${m}m` : `${h}h`;
}
