import { DateData, Department, Holiday, Shift, ShiftTemplate, WorkDay } from '@/types'
import { dayNames, isHoliday, isWorkingDay } from '@/helpers/dateHelper'

export type IssueSeverity = 'understaffed' | 'overstaffed'

export type DayIssue = {
    isoDate: string
    label: string
    assigned: number
    required: number
    severity: IssueSeverity
}

export type ShiftTemplateCoverage = {
    shiftType: ShiftTemplate
    departmentName: string
    totalWorkingDays: number
    okDays: number
    issues: DayIssue[]
}

function isDepartmentWorkingDay(isoDate: string, department: Department | undefined): boolean {
    if (!department || department.workingDays.length === 0) return true
    const dayOfWeek = new Date(isoDate + 'T00:00:00').getDay()
    const dayName = dayNames[dayOfWeek]
    return (department.workingDays as unknown as string[]).includes(dayName)
}

export function computeCoverage(
    shiftTypes: ShiftTemplate[],
    shiftsData: Shift[],
    daysOfMonth: DateData[],
    orgHolidays: Holiday[],
    orgSchedule: WorkDay[],
    departments: Department[],
): ShiftTemplateCoverage[] {
    const orgWorkingDays = daysOfMonth.filter(
        day => !isHoliday(day.isoDate, orgHolidays) && isWorkingDay(day.isoDate, orgSchedule)
    )

    return shiftTypes.map(st => {
        const department = departments.find(d => d.id === st.departmentId)

        const relevantDays = orgWorkingDays.filter(day =>
            isDepartmentWorkingDay(day.isoDate, department)
        )

        const issues: DayIssue[] = []

        relevantDays.forEach(day => {
            const shift = shiftsData.find(
                s => s.date === day.isoDate && s.shiftTypeId === st.id
            )
            const assigned = shift?.employees.length ?? 0

            if (assigned < st.minEmployees) {
                issues.push({
                    isoDate: day.isoDate,
                    label: day.label,
                    assigned,
                    required: st.minEmployees,
                    severity: 'understaffed',
                })
            } else if (assigned > st.maxEmployees) {
                issues.push({
                    isoDate: day.isoDate,
                    label: day.label,
                    assigned,
                    required: st.maxEmployees,
                    severity: 'overstaffed',
                })
            }
        })

        return {
            shiftType: st,
            departmentName: department?.name ?? '—',
            totalWorkingDays: relevantDays.length,
            okDays: relevantDays.length - issues.filter(i => i.severity === 'understaffed').length,
            issues,
        }
    })
}
