'use client'

import {Fragment, useMemo} from 'react'
import { toast } from 'sonner'
import { DateData, Department, EmployeeMinData, Holiday, Shift, ShiftTemplate, WorkDay } from '@/types'
import { EmployeeShiftAssignment, EmployeeTimeOff } from '@/types/schedule'
import {
    Table,
    TableBody,
    TableCell,
    TableHead,
    TableHeader,
    TableRow,
} from '@/components/ui/shadcn/table'
import { MonthNavigator } from '@/components/features/schedules/MonthNavigator'
import MatrixCell from './MatrixCell'
import DraggableEmployee from './DraggableEmployee'
import ScheduleEmployees from '@/components/features/schedules/ScheduleEmployees'

type ScheduleMatrixProps = {
    employees: EmployeeMinData[]
    shiftTypes: ShiftTemplate[]
    departments?: Department[]
    shiftsData: Shift[]
    setShiftsData: (shifts: Shift[]) => void
    daysOfMonth: DateData[]
    currentMonth: number
    currentYear: number
    setCurrentMonth: (month: number) => void
    setCurrentYear: (year: number) => void
    isConfirmed: boolean
    orgHolidays?: Holiday[]
    orgSchedule?: WorkDay[]
    employeeTimeOffs?: EmployeeTimeOff[]
}

export default function ScheduleMatrix({
    employees,
    shiftTypes,
    departments = [],
    shiftsData,
    setShiftsData,
    daysOfMonth,
    currentMonth,
    currentYear,
    setCurrentMonth,
    setCurrentYear,
    isConfirmed,
    orgHolidays = [],
}: ScheduleMatrixProps) {
    const groupedShifts = useMemo(() => {
        const withDept = departments
            .map(dept => ({
                department: dept,
                templates: shiftTypes.filter(st => st.departmentId === dept.id),
            }))
            .filter(g => g.templates.length > 0)

        const unassigned = shiftTypes.filter(st => !departments.some(d => d.id === st.departmentId))

        return unassigned.length > 0
            ? [...withDept, { department: null as Department | null, templates: unassigned }]
            : withDept
    }, [shiftTypes, departments])

    const assignEmployee = (employeeId: number, employeeName: string, date: string, shiftTypeId: number) => {
        const shiftType = shiftTypes.find(st => st.id === shiftTypeId)
        if (!shiftType) return

        if (shiftsData.some(s => s.date === date && s.employees.some(e => e.id === employeeId))) {
            toast.warning('Employee already has a shift on this day')
            return
        }

        const existing = shiftsData.find(s => s.date === date && s.shiftTypeId === shiftTypeId)
        if (existing) {
            setShiftsData(shiftsData.map(s =>
                s.id === existing.id
                    ? { ...s, employees: [...s.employees, { id: employeeId, name: employeeName }] }
                    : s
            ))
        } else {
            setShiftsData([
                ...shiftsData,
                {
                    id: Date.now(),
                    date,
                    shiftTypeId: shiftType.id,
                    shiftTypeName: shiftType.name,
                    startTime: shiftType.startTime,
                    endTime: shiftType.endTime,
                    color: shiftType.color,
                    employeeNeeded: shiftType.minEmployees,
                    employees: [{ id: employeeId, name: employeeName }],
                },
            ])
        }
    }

    const moveEmployee = (
        employeeId: number,
        fromDate: string,
        fromShiftTypeId: number,
        toDate: string,
        toShiftTypeId: number,
    ) => {
        if (fromDate === toDate && fromShiftTypeId === toShiftTypeId) return

        if (shiftsData.some(s => s.date === toDate && s.employees.some(e => e.id === employeeId))) {
            toast.warning('Employee already has a shift on this day')
            return
        }

        const employeeName = employees.find(e => e.id === employeeId)?.name ?? ''
        const toShiftType = shiftTypes.find(st => st.id === toShiftTypeId)
        if (!toShiftType) return

        let newShifts = shiftsData
            .map(s =>
                s.date === fromDate && s.shiftTypeId === fromShiftTypeId
                    ? { ...s, employees: s.employees.filter(e => e.id !== employeeId) }
                    : s
            )
            .filter(s => s.employees.length > 0)

        const existingTarget = newShifts.find(s => s.date === toDate && s.shiftTypeId === toShiftTypeId)
        if (existingTarget) {
            newShifts = newShifts.map(s =>
                s.id === existingTarget.id
                    ? { ...s, employees: [...s.employees, { id: employeeId, name: employeeName }] }
                    : s
            )
        } else {
            newShifts = [
                ...newShifts,
                {
                    id: Date.now(),
                    date: toDate,
                    shiftTypeId: toShiftType.id,
                    shiftTypeName: toShiftType.name,
                    startTime: toShiftType.startTime,
                    endTime: toShiftType.endTime,
                    color: toShiftType.color,
                    employeeNeeded: toShiftType.minEmployees,
                    employees: [{ id: employeeId, name: employeeName }],
                },
            ]
        }

        setShiftsData(newShifts)
    }

    const removeEmployee = (employeeId: number, date: string, shiftTypeId: number) => {
        setShiftsData(
            shiftsData
                .map(s =>
                    s.date === date && s.shiftTypeId === shiftTypeId
                        ? { ...s, employees: s.employees.filter(e => e.id !== employeeId) }
                        : s
                )
                .filter(s => s.employees.length > 0)
        )
    }

    const getEmployees = (date: string, shiftTypeId: number): EmployeeShiftAssignment[] =>
        shiftsData.find(s => s.date === date && s.shiftTypeId === shiftTypeId)?.employees ?? []

    const isNonWorking = (date: string) => {
        const d = new Date(date)
        const h = orgHolidays.find(h => h.month === d.getUTCMonth() + 1 && h.day === d.getUTCDate())
        return !!h && !h.isShortenedDay
    }

    const isWeekendDay = (date: string) => {
        const d = new Date(date)
        return d.getUTCDay() === 0 || d.getUTCDay() === 6
    }

    const employeesSidebar = (
        <div>
            <h4 className="font-semibold text-sm mb-3">Employees</h4>
            <div className="flex flex-col gap-1.5">
                {employees.map(emp => (
                    <DraggableEmployee
                        key={emp.id}
                        employeeId={emp.id}
                        employeeName={emp.name}
                        departmentNames={emp.departmentNames}
                    />
                ))}
            </div>
        </div>
    )

    return (
        <div className="flex flex-col h-full overflow-hidden">
            <ScheduleEmployees
                isEditable={true}
                layoutPosition="left"
                employees={employees}
                sidebarChildren={employeesSidebar}
            >
                <div className="flex-1 flex flex-col overflow-hidden">
                    <div className="flex items-center gap-3 px-3 py-2 border-b shrink-0">
                        <MonthNavigator
                            currentMonth={currentMonth}
                            currentYear={currentYear}
                            isConfirmed={isConfirmed}
                            onChange={(month, year) => {
                                setCurrentMonth(month)
                                setCurrentYear(year)
                            }}
                        />
                        <div className="flex items-center gap-3 ml-2 text-[11px] text-muted-foreground">
                            <span className="flex items-center gap-1"><span className="w-3 h-3 rounded-sm bg-red-50 dark:bg-red-950/40 border border-red-200 inline-block" /> Understaffed</span>
                            <span className="flex items-center gap-1"><span className="w-3 h-3 rounded-sm bg-amber-50 dark:bg-amber-950/40 border border-amber-200 inline-block" /> Partial</span>
                            <span className="flex items-center gap-1"><span className="w-3 h-3 rounded-sm bg-green-50 dark:bg-green-950/30 border border-green-200 inline-block" /> Full</span>
                        </div>
                    </div>

                    <div className="flex-1 overflow-auto">
                        <Table noWrapper>
                            <TableHeader className="sticky top-0 z-20 bg-background">
                                <TableRow>
                                    <TableHead className="sticky left-0 z-30 bg-background border-r w-[190px] min-w-[190px]">
                                        Shift / Day
                                    </TableHead>
                                    {daysOfMonth.map(day => {
                                        const holiday = isNonWorking(day.isoDate)
                                        const weekend = isWeekendDay(day.isoDate)
                                        return (
                                            <TableHead
                                                key={day.isoDate}
                                                className={`text-center min-w-[130px] text-xs border-r ${
                                                    holiday
                                                        ? 'bg-red-100 dark:bg-red-950 text-red-600 dark:text-red-400'
                                                        : weekend
                                                            ? 'bg-yellow-50/50 dark:bg-yellow-900/10 ring-1 ring-inset ring-yellow-300/60 dark:ring-yellow-700/50'
                                                            : ''
                                                }`}
                                            >
                                                {day.label}
                                            </TableHead>
                                        )
                                    })}
                                </TableRow>
                            </TableHeader>
                            <TableBody>
                                {groupedShifts.map(({ department, templates }) => (
                                    <Fragment key={department?.id}>
                                        <TableRow key={`dept-${department?.id ?? 'other'}`} className="bg-muted/40 hover:bg-muted/40">
                                            <TableCell
                                                className="sticky left-0 py-1.5 px-3 text-xs font-semibold border-r"
                                                style={department?.color ? { color: department.color } : undefined}
                                            >
                                                <span
                                                    className="inline-block w-2 h-2 rounded-full mr-1.5"
                                                    style={department?.color ? { backgroundColor: department.color } : undefined}
                                                />
                                                {department?.name ?? 'Other'}
                                            </TableCell>
                                        </TableRow>
                                        {templates.map(template => (
                                            <TableRow key={template.id}>
                                                <TableCell className="sticky left-0 bg-background border-r z-10 px-3 py-2 align-top w-[190px] min-w-[190px]">
                                                    <div
                                                        className="text-sm font-semibold leading-tight truncate"
                                                        style={{ color: template.color }}
                                                    >
                                                        {template.name}
                                                    </div>
                                                    <div className="text-xs text-muted-foreground mt-0.5">
                                                        {template.startTime.slice(0, 5)} – {template.endTime.slice(0, 5)}
                                                    </div>
                                                    <div className="text-xs text-muted-foreground">
                                                        {template.minEmployees}–{template.maxEmployees} сотр.
                                                    </div>
                                                </TableCell>
                                                {daysOfMonth.map(day => (
                                                    <TableCell key={day.isoDate} className="p-0 border-r align-top">
                                                        <MatrixCell
                                                            date={day.isoDate}
                                                            shiftTypeId={template.id}
                                                            shiftColor={template.color}
                                                            employees={getEmployees(day.isoDate, template.id)}
                                                            minEmployees={template.minEmployees}
                                                            maxEmployees={template.maxEmployees}
                                                            isNonWorkingDay={isNonWorking(day.isoDate)}
                                                            isWeekend={isWeekendDay(day.isoDate)}
                                                            showWeekendHighlight={true}
                                                            onAssign={assignEmployee}
                                                            onMove={moveEmployee}
                                                            onRemove={removeEmployee}
                                                        />
                                                    </TableCell>
                                                ))}
                                            </TableRow>
                                        ))}
                                    </Fragment>
                                ))}
                            </TableBody>
                        </Table>
                    </div>
                </div>
            </ScheduleEmployees>
        </div>
    )
}
