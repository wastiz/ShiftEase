'use client'

import { Button } from '@/components/ui/shadcn/button'
import { PanelLeft, PanelTop } from 'lucide-react'
import DayContainer from './DayContainer'
import { DateData, EmployeeMinData, Department, Holiday, Shift, ShiftTemplate, WorkDay } from '@/types'
import { EmployeeTimeOff } from '@/types/schedule'
import { Dispatch, SetStateAction, useState, useMemo } from 'react'
import { WarningMessage } from "@/app/(private)/(employer)/schedules/manage/page"
import { MonthNavigator } from "@/components/features/schedules/MonthNavigator"
import { MessageIndicator } from "@/components/features/schedules/MessageIndicator"
import ScheduleShiftTemplates from "@/components/features/schedules/ScheduleShiftTemplates"
import ScheduleEmployees from "@/components/features/schedules/ScheduleEmployees"
import { ScheduleFilter } from "@/components/features/schedules/ScheduleFilter"
import { EmployeeFilter } from "@/components/features/schedules/EmployeeFilter"
import { HighlightSettingsPopover } from "@/components/features/schedules/HighlightSettingsPopover"

type ScheduleCalendarProps = {
    shiftsData: Shift[]
    setShiftsData?: Dispatch<SetStateAction<Shift[]>>
    shiftTypes: ShiftTemplate[]
    employees: EmployeeMinData[]
    departments?: Department[]
    daysOfMonth: DateData[]
    currentMonth: number
    currentYear: number
    setCurrentMonth: Dispatch<SetStateAction<number>>
    setCurrentYear: Dispatch<SetStateAction<number>>
    isConfirmed: boolean
    isEditable?: boolean
    cellHeight?: number
    orgHolidays?: Holiday[]
    orgSchedule?: WorkDay[]
    employeeTimeOffs?: EmployeeTimeOff[]
    warningMessage: WarningMessage | null
}

export default function ScheduleCalendar({
    shiftsData,
    setShiftsData,
    shiftTypes,
    employees,
    departments = [],
    daysOfMonth,
    currentMonth,
    currentYear,
    setCurrentMonth,
    setCurrentYear,
    isConfirmed,
    isEditable = true,
    cellHeight = 40,
    orgHolidays,
    orgSchedule,
    employeeTimeOffs = [],
    warningMessage,
}: ScheduleCalendarProps) {

    const [layoutPosition, setLayoutPosition] = useState<'left' | 'top'>('left')
    const [selectedShiftTemplateIds, setSelectedShiftTemplateIds] = useState<number[]>([])
    const [selectedDepartmentId, setSelectedDepartmentId] = useState<number | null>(null)
    const [selectedEmployeeId, setSelectedEmployeeId] = useState<number | null>(null)

    const [showWeekendHighlight, setShowWeekendHighlight] = useState(true)
    const [showHolidayHighlight, setShowHolidayHighlight] = useState(true)
    const [showShortenedHighlight, setShowShortenedHighlight] = useState(true)

    const selectedDepartment = useMemo(
        () => departments.find(g => g.id === selectedDepartmentId) ?? null,
        [departments, selectedDepartmentId]
    )

    const departmentShiftTemplates = useMemo(
        () => selectedDepartmentId !== null
            ? shiftTypes.filter(st => st.departmentId === selectedDepartmentId)
            : shiftTypes,
        [shiftTypes, selectedDepartmentId]
    )

    const departmentEmployees = useMemo(
        () => selectedDepartment
            ? employees.filter(emp => emp.departmentNames.includes(selectedDepartment.name))
            : employees,
        [employees, selectedDepartment]
    )

    const departmentShifts = useMemo(() => {
        const departmentShiftTemplateIds = new Set(departmentShiftTemplates.map(st => st.id))
        return shiftsData.filter(s => departmentShiftTemplateIds.has(s.shiftTypeId))
    }, [shiftsData, departmentShiftTemplates])

    const visibleEmployees = useMemo(() => {
        let result = departmentEmployees

        if (selectedShiftTemplateIds.length > 0) {
            const empIds = new Set<number>()
            departmentShifts.forEach(shift => {
                if (selectedShiftTemplateIds.includes(shift.shiftTypeId)) {
                    shift.employees.forEach(e => empIds.add(e.id))
                }
            })
            result = result.filter(emp => empIds.has(emp.id))
        }

        if (selectedEmployeeId !== null) {
            result = result.filter(emp => emp.id === selectedEmployeeId)
        }

        return result
    }, [departmentEmployees, selectedShiftTemplateIds, departmentShifts, selectedEmployeeId])

    const visibleShiftTemplates = useMemo(
        () => selectedShiftTemplateIds.length > 0
            ? departmentShiftTemplates.filter(st => selectedShiftTemplateIds.includes(st.id))
            : departmentShiftTemplates,
        [departmentShiftTemplates, selectedShiftTemplateIds]
    )

    return (
        <div className="flex flex-col h-[calc(100vh-62px)]">

            <div className="flex items-center gap-3 flex-shrink-0 m-4">
                <MonthNavigator
                    currentMonth={currentMonth}
                    currentYear={currentYear}
                    isConfirmed={isConfirmed}
                    onChange={(month, year) => {
                        setCurrentMonth(month)
                        setCurrentYear(year)
                    }}
                />

                <EmployeeFilter
                    employees={departmentEmployees}
                    selectedEmployeeId={selectedEmployeeId}
                    onSelect={setSelectedEmployeeId}
                />

                {isEditable && (
                    <>
                        <ScheduleFilter
                            departments={departments}
                            shiftTypes={shiftTypes}
                            selectedDepartmentId={selectedDepartmentId}
                            selectedShiftTemplates={selectedShiftTemplateIds}
                            onToggleDepartment={(id) =>
                                setSelectedDepartmentId(prev => prev === id ? null : id)
                            }
                            onToggleShiftTemplate={(id) =>
                                setSelectedShiftTemplateIds(prev =>
                                    prev.includes(id) ? prev.filter(x => x !== id) : [...prev, id]
                                )
                            }
                            onClear={() => {
                                setSelectedShiftTemplateIds([])
                                setSelectedDepartmentId(null)
                                setSelectedEmployeeId(null)
                            }}
                        />

                        <HighlightSettingsPopover
                            showWeekendHighlight={showWeekendHighlight}
                            showHolidayHighlight={showHolidayHighlight}
                            showShortenedHighlight={showShortenedHighlight}
                            onToggleWeekend={setShowWeekendHighlight}
                            onToggleHoliday={setShowHolidayHighlight}
                            onToggleShortened={setShowShortenedHighlight}
                        />

                        <Button
                            variant="outline"
                            size="icon"
                            onClick={() => setLayoutPosition(p => p === 'left' ? 'top' : 'left')}
                        >
                            {layoutPosition === 'left' ? <PanelTop /> : <PanelLeft />}
                        </Button>
                    </>
                )}

                {warningMessage && (
                    <MessageIndicator
                        message={warningMessage.message}
                        messageType="warning"
                    />
                )}
            </div>

            <div className="flex-1 min-h-0 flex flex-col px-4 pb-4">
                <ScheduleShiftTemplates
                    isEditable={isEditable}
                    layoutPosition={layoutPosition}
                    shiftTypes={visibleShiftTemplates}
                >
                    <ScheduleEmployees
                        isEditable={isEditable}
                        layoutPosition={layoutPosition}
                        employees={visibleEmployees}
                    >
                        <section className="flex-1 flex flex-col min-w-0">
                            <div className="flex-1 border rounded-lg overflow-auto">
                                <div
                                    className="grid min-w-max"
                                    style={{
                                        gridTemplateColumns: `100px repeat(${daysOfMonth.length}, 180px)`,
                                    }}
                                >
                                    {/* Time column */}
                                    <div className="sticky left-0 z-10 shadow-md bg-background">
                                        <div className="h-10 flex items-center justify-center font-medium border-b border-r bg-black" />
                                        {Array.from({ length: 24 }, (_, i) => `${i.toString().padStart(2, '0')}:00`).map(h => (
                                            <div
                                                key={h}
                                                className="h-10 bg-black border-b border-r flex items-center justify-center text-sm"
                                            >
                                                {h}
                                            </div>
                                        ))}
                                    </div>

                                    {daysOfMonth.map(day => (
                                        <DayContainer
                                            key={day.isoDate}
                                            date={day.isoDate}
                                            dateLabel={day.label}
                                            shiftTypes={departmentShiftTemplates}
                                            employees={departmentEmployees}
                                            shiftsData={departmentShifts}
                                            setShiftsData={setShiftsData}
                                            isEditable={isEditable}
                                            cellHeight={cellHeight}
                                            holidays={orgHolidays}
                                            workDays={orgSchedule}
                                            employeeTimeOffs={employeeTimeOffs}
                                            showWeekendHighlight={showWeekendHighlight}
                                            showHolidayHighlight={showHolidayHighlight}
                                            showShortenedHighlight={showShortenedHighlight}
                                        />
                                    ))}
                                </div>
                            </div>
                        </section>
                    </ScheduleEmployees>
                </ScheduleShiftTemplates>
            </div>
        </div>
    )
}
