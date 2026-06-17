"use client";

import { useMemo, useState, useCallback } from "react";
import {
    Table,
    TableBody,
    TableCell,
    TableHead,
    TableHeader,
    TableRow,
} from "@/components/ui/shadcn/table";
import { Badge } from "@/components/ui/shadcn/badge";
import { Button } from "@/components/ui/shadcn/button";
import { ClipboardCheck, Maximize2, Minimize2, PanelLeft, PanelTop, X } from "lucide-react";
import { DateData, EmployeeMinData, Department, Holiday, Shift, ShiftTemplate, WorkDay } from "@/types";
import { EmployeeTimeOff } from "@/types/schedule";
import { transformToSimpleView } from "@/helpers/scheduleHelper";
import { toast } from "sonner";
import {
    Popover,
    PopoverContent,
    PopoverTrigger,
} from "@/components/ui/shadcn/popover";
import DroppableCell from "@/components/features/schedules/ScheduleSimple/DroppableCell";
import ScheduleShiftTemplates from "@/components/features/schedules/ScheduleShiftTemplates";
import { useTranslations } from 'next-intl';
import {
    getEmployeeTimeOff,
    getHolidayName,
    getTimeOffColor,
    getTimeOffLabelKey,
    timeToMinutes,
} from "@/helpers/dateHelper";
import { WarningMessage } from "@/app/(private)/(employer)/schedules/manage/page";
import { MonthNavigator } from "@/components/features/schedules/MonthNavigator";
import { MessageIndicator } from "@/components/features/schedules/MessageIndicator";
import { CoverageCheckModal } from "@/components/features/schedules/CoverageCheckModal";
import {EmployeeFilter} from "@/components/features/schedules/EmployeeFilter";
import {ScheduleFilter} from "@/components/features/schedules/ScheduleFilter";
import {HighlightSettingsPopover} from "@/components/features/schedules/HighlightSettingsPopover";

type SimpleViewProps = {
    employees: EmployeeMinData[];
    shiftTypes: ShiftTemplate[];
    departments?: Department[];
    shiftsData: Shift[];
    setShiftsData: (shifts: Shift[]) => void;
    daysOfMonth: DateData[];
    currentMonth: number;
    currentYear: number;
    setCurrentMonth: (month: number) => void;
    setCurrentYear: (year: number) => void;
    isConfirmed: boolean;
    orgHolidays?: Holiday[];
    orgSchedule?: WorkDay[];
    employeeTimeOffs?: EmployeeTimeOff[];
    warningMessage: WarningMessage | null;
};

export default function SimpleView({
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
    orgSchedule = [],
    employeeTimeOffs = [],
    warningMessage,
}: SimpleViewProps) {
    const t = useTranslations('schedule');
    const dates = daysOfMonth.map((d) => d.isoDate);
    const [selectedShiftTemplates, setSelectedShiftTemplates] = useState<number[]>([]);
    const [selectedDepartmentId, setSelectedDepartmentId] = useState<number | null>(null);
    const [selectedEmployeeId, setSelectedEmployeeId] = useState<number | null>(null);
    const [coverageModalOpen, setCoverageModalOpen] = useState(false);
    const [isCompact, setIsCompact] = useState(false);
    const [layoutPosition, setLayoutPosition] = useState<'left' | 'top'>('top');

    const [showWeekendHighlight, setShowWeekendHighlight] = useState(true);
    const [showHolidayHighlight, setShowHolidayHighlight] = useState(true);
    const [showShortenedHighlight, setShowShortenedHighlight] = useState(true);

    const filteredEmployees = useMemo(() => {
        let result = employees;

        if (selectedDepartmentId !== null) {
            const department = departments.find(g => g.id === selectedDepartmentId);
            if (department) {
                result = result.filter(emp => emp.departmentNames.includes(department.name));
            }
        }

        if (selectedShiftTemplates.length > 0) {
            const employeeIdsWithSelectedShifts = new Set<number>();
            shiftsData.forEach(shift => {
                if (selectedShiftTemplates.includes(shift.shiftTypeId)) {
                    shift.employees.forEach(emp => employeeIdsWithSelectedShifts.add(emp.id));
                }
            });
            result = result.filter(emp => employeeIdsWithSelectedShifts.has(emp.id));
        }

        if (selectedEmployeeId !== null) {
            result = result.filter(emp => emp.id === selectedEmployeeId);
        }

        return result;
    }, [employees, departments, selectedDepartmentId, selectedShiftTemplates, shiftsData, selectedEmployeeId]);

    const rows = useMemo(
        () => transformToSimpleView(filteredEmployees, shiftsData, dates, shiftTypes),
        [filteredEmployees, shiftsData, dates, shiftTypes]
    );

    const weeks = useMemo<(DateData | null)[][]>(() => {
        if (!daysOfMonth.length) return []
        const firstDayOfWeek = new Date(daysOfMonth[0].isoDate + 'T00:00:00').getDay()
        // Monday-first offset: Mon=0, Tue=1, ..., Sun=6
        const startOffset = (firstDayOfWeek + 6) % 7
        const result: (DateData | null)[][] = []
        let week: (DateData | null)[] = Array(startOffset).fill(null)
        for (const day of daysOfMonth) {
            week.push(day)
            if (week.length === 7) {
                result.push(week)
                week = []
            }
        }
        if (week.length > 0) {
            while (week.length < 7) week.push(null)
            result.push(week)
        }
        return result
    }, [daysOfMonth])

    const handleEmployeeDoubleClick = useCallback((employeeId: number) => {
        setSelectedEmployeeId(prev => prev === employeeId ? null : employeeId)
    }, [])

    const handleShiftTemplateFilterToggle = (shiftTypeId: number) => {
        setSelectedShiftTemplates(prev =>
            prev.includes(shiftTypeId)
                ? prev.filter(id => id !== shiftTypeId)
                : [...prev, shiftTypeId]
        );
    };

    const handleDepartmentFilterToggle = (departmentId: number) => {
        setSelectedDepartmentId(prev => prev === departmentId ? null : departmentId);
    };

    const assignShift = (employeeId: number, date: string, shiftTypeId: number) => {
        const shiftType = shiftTypes.find((st) => st.id === shiftTypeId);
        if (!shiftType) {
            toast.error("Shift type not found");
            return;
        }

        const existingShift = shiftsData.find(
            (s) => s.date === date && s.employees.some((e) => e.id === employeeId)
        );

        if (existingShift) {
            toast.warning("Employee already has a shift on this day");
            return;
        }

        let sTime = shiftType.startTime
        let eTime = shiftType.endTime

        const holidayInfo = orgHolidays.find((h) => {
            const d = new Date(date)
            return h.month === d.getUTCMonth() + 1 && h.day === d.getUTCDate()
        })
        const isShortenedDay = holidayInfo?.isShortenedDay

        if (isShortenedDay && holidayInfo?.startTime && holidayInfo?.endTime) {
            const shiftStartM = timeToMinutes(sTime)
            const shiftEndM = timeToMinutes(eTime)
            const hStartM = timeToMinutes(holidayInfo.startTime)
            const hEndM = timeToMinutes(holidayInfo.endTime)

            if (shiftStartM < shiftEndM && hStartM < hEndM) {
                if (shiftStartM < hStartM) sTime = holidayInfo.startTime
                if (shiftEndM > hEndM) eTime = holidayInfo.endTime
                toast.info('Shift adjusted to shortened holiday hours')
            }
        }

        const existingShiftForType = shiftsData.find(
            (s) => s.date === date && s.shiftTypeId === shiftTypeId
        );

        if (existingShiftForType) {
            const updatedShifts = shiftsData.map((s) =>
                s.id === existingShiftForType.id
                    ? {
                        ...s,
                        employees: [
                            ...s.employees,
                            {
                                id: employeeId,
                                name: employees.find((e) => e.id === employeeId)?.name || "",
                                note: undefined,
                            },
                        ],
                    }
                    : s
            );
            setShiftsData(updatedShifts);
        } else {
            const newShift: Shift = {
                id: Date.now(),
                date,
                shiftTypeId: shiftType.id,
                shiftTypeName: shiftType.name,
                startTime: sTime,
                endTime: eTime,
                color: shiftType.color,
                employeeNeeded: 1,
                employees: [
                    {
                        id: employeeId,
                        name: employees.find((e) => e.id === employeeId)?.name || "",
                        note: undefined,
                    },
                ],
            };
            setShiftsData([...shiftsData, newShift]);
        }
    };

    const removeShift = (employeeId: number, date: string) => {
        const updatedShifts = shiftsData
            .map((shift) => {
                if (shift.date !== date) return shift;

                const updatedEmployees = shift.employees.filter((e) => e.id !== employeeId);

                if (updatedEmployees.length === 0) return null;

                return { ...shift, employees: updatedEmployees };
            })
            .filter((shift): shift is Shift => shift !== null);

        setShiftsData(updatedShifts);
    };

    const updateNote = (employeeId: number, date: string, note: string) => {
        const updatedShifts = shiftsData.map((shift) => {
            if (shift.date !== date) return shift;

            const hasEmployee = shift.employees.some((e) => e.id === employeeId);
            if (!hasEmployee) return shift;

            return {
                ...shift,
                employees: shift.employees.map((e) =>
                    e.id === employeeId ? { ...e, note } : e
                ),
            };
        });

        setShiftsData(updatedShifts);
        toast.success('Note updated');
    };

    return (
        <div className="flex flex-col gap-4 p-4 h-full overflow-hidden">

            <div className="flex items-center gap-3 flex-shrink-0">
                <MonthNavigator
                    currentMonth={currentMonth}
                    currentYear={currentYear}
                    isConfirmed={isConfirmed}
                    onChange={(month, year) => {
                        setCurrentMonth(month);
                        setCurrentYear(year);
                    }}
                />

                <Button
                    variant="outline"
                    size="icon"
                    onClick={() => setCoverageModalOpen(true)}
                    title="Coverage Check"
                >
                    <ClipboardCheck className="h-4 w-4" />
                </Button>

                {warningMessage && (
                    <MessageIndicator
                        message={warningMessage.message}
                        messageType="warning"
                    />
                )}


                {/* Employee search combobox */}
                <EmployeeFilter
                    employees={employees}
                    selectedEmployeeId={selectedEmployeeId}
                    onSelect={setSelectedEmployeeId}
                />

                {/* ScheduleFilter */}
                <ScheduleFilter
                    departments={departments}
                    shiftTypes={shiftTypes}
                    selectedDepartmentId={selectedDepartmentId}
                    selectedShiftTemplates={selectedShiftTemplates}
                    onToggleDepartment={handleDepartmentFilterToggle}
                    onToggleShiftTemplate={handleShiftTemplateFilterToggle}
                    onClear={() => {
                        setSelectedShiftTemplates([]);
                        setSelectedDepartmentId(null);
                        setSelectedEmployeeId(null);
                    }}
                />

                {/*Highlight Setting Popover*/}
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
                    title={layoutPosition === 'left' ? "Top Layout" : "Left Layout"}
                >
                    {layoutPosition === 'left' ? <PanelTop className="h-4 w-4" /> : <PanelLeft className="h-4 w-4" />}
                </Button>

                <Button
                    variant="outline"
                    size="icon"
                    onClick={() => setIsCompact(!isCompact)}
                    title={isCompact ? t('fullView') : t('compactView')}
                >
                    {isCompact ? <Maximize2 className="h-4 w-4" /> : <Minimize2 className="h-4 w-4" />}
                </Button>
            </div>

            <ScheduleShiftTemplates
                isEditable={true}
                layoutPosition={layoutPosition}
                shiftTypes={shiftTypes}
            >
                {selectedEmployeeId !== null && rows.length === 1 ? (
                    /* ── Calendar grid view for focused employee ── */
                    <div className="flex-1 border rounded-lg overflow-auto p-3 flex flex-col gap-2">
                        {/* Employee header */}
                        <div className="flex items-center gap-3 px-1 pb-1 border-b">
                            <span className="font-semibold text-sm">{rows[0].employeeName}</span>
                            {rows[0].departmentNames.map(n => (
                                <span key={n} className="text-xs text-muted-foreground">{n}</span>
                            ))}
                            <Badge
                                className="ml-auto"
                                variant={rows[0].totalHours > 200 ? "destructive" : rows[0].totalHours > 180 ? "warning" : "secondary"}
                            >
                                {rows[0].totalHours.toFixed(1)}h
                            </Badge>
                            <button
                                className="text-muted-foreground hover:text-foreground"
                                onClick={() => setSelectedEmployeeId(null)}
                                title="Exit focus mode"
                            >
                                <X className="h-3.5 w-3.5" />
                            </button>
                        </div>
                        {/* Day-of-week header */}
                        <div className="grid grid-cols-7 gap-1">
                            {['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'].map(d => (
                                <div key={d} className="text-center text-xs font-medium text-muted-foreground py-1">{d}</div>
                            ))}
                        </div>
                        {/* Weeks */}
                        {weeks.map((week, wi) => (
                            <div key={wi} className="grid grid-cols-7 gap-1">
                                {week.map((day, di) => {
                                    if (!day) return <div key={di} className="min-h-[80px] rounded-md bg-muted/20" />
                                    const holidayInfo = orgHolidays.find(h => {
                                        const d = new Date(day.isoDate)
                                        return h.month === d.getUTCMonth() + 1 && h.day === d.getUTCDate()
                                    })
                                    const dayDate = new Date(day.isoDate)
                                    const isWeekend = dayDate.getUTCDay() === 0 || dayDate.getUTCDay() === 6
                                    const isHolidayDay = !!holidayInfo && !holidayInfo.isShortenedDay
                                    const isShortenedDay = !!holidayInfo?.isShortenedDay
                                    const timeOff = getEmployeeTimeOff(rows[0].employeeId, day.isoDate, employeeTimeOffs)
                                    const assignment = rows[0].days.get(day.isoDate)

                                    return (
                                        <div
                                            key={day.isoDate}
                                            className={`min-h-[80px] rounded-md border flex flex-col overflow-hidden ${
                                                isHolidayDay && showHolidayHighlight ? 'bg-red-50 dark:bg-red-950/20 border-red-200 dark:border-red-900' :
                                                isShortenedDay && showShortenedHighlight ? 'bg-orange-50 dark:bg-orange-950/20 border-orange-200' :
                                                isWeekend && showWeekendHighlight ? 'bg-yellow-50/40 dark:bg-yellow-900/10 border-yellow-200/60' :
                                                'border-border'
                                            } ${timeOff ? getTimeOffColor(timeOff.type) : ''}`}
                                        >
                                            {/* Date label */}
                                            <div className={`px-1.5 py-0.5 text-[11px] font-medium flex items-center justify-between ${
                                                isHolidayDay && showHolidayHighlight ? 'text-red-600 dark:text-red-400' :
                                                isShortenedDay && showShortenedHighlight ? 'text-orange-500' :
                                                isWeekend ? 'text-yellow-700 dark:text-yellow-400' :
                                                'text-muted-foreground'
                                            }`}>
                                                <span>{day.label.split(' ')[0]}</span>
                                                {isShortenedDay && !isHolidayDay && (
                                                    <span className="text-[9px]">{holidayInfo?.startTime}-{holidayInfo?.endTime}</span>
                                                )}
                                            </div>
                                            {/* Cell content */}
                                            <div className="flex-1 p-0.5">
                                                {timeOff ? (
                                                    <div className="text-xs text-center py-1 px-1 rounded border h-full flex items-center justify-center">
                                                        <span className="font-medium text-[10px]">{t(getTimeOffLabelKey(timeOff.type))}</span>
                                                    </div>
                                                ) : (
                                                    <DroppableCell
                                                        employeeId={rows[0].employeeId}
                                                        date={day.isoDate}
                                                        assignment={assignment}
                                                        onAssign={assignShift}
                                                        onRemove={removeShift}
                                                        onUpdateNote={updateNote}
                                                        isNonWorkingDay={isHolidayDay}
                                                        isCompact={false}
                                                    />
                                                )}
                                            </div>
                                        </div>
                                    )
                                })}
                            </div>
                        ))}
                    </div>
                ) : (
                <div className={`flex-1 border rounded-lg overflow-auto ${isCompact ? "overflow-x-hidden" : ""}`}>
                    <Table noWrapper className={isCompact ? "table-fixed w-full" : ""}>
                        <TableHeader className="sticky top-0 z-20 bg-background">
                            <TableRow>
                                <TableHead className={`${isCompact ? "w-[120px] px-2 text-[10px]" : "w-[180px]"} sticky top-0 left-0 bg-background z-30 border-r`}>
                                    <div className="flex items-center justify-between gap-1">
                                        <span className="truncate">Employees</span>
                                        <Badge variant="secondary" className={isCompact ? "px-1 text-[9px]" : ""}>
                                            {rows.reduce((sum, r) => sum + r.totalHours, 0).toFixed(isCompact ? 0 : 1)}h
                                        </Badge>
                                    </div>
                                </TableHead>
                                {daysOfMonth.map((day) => {
                                    const holidayInfo = orgHolidays.find((h) => {
                                        const d = new Date(day.isoDate)
                                        return h.month === d.getUTCMonth() + 1 && h.day === d.getUTCDate()
                                    })
                                    const dayDate = new Date(day.isoDate)
                                    const isWeekend = dayDate.getUTCDay() === 0 || dayDate.getUTCDay() === 6
                                    const holiday = !!holidayInfo
                                    const isShortenedDay = holidayInfo?.isShortenedDay
                                    const isNonWorkingDay = (holiday && !isShortenedDay)

                                    return (
                                        <TableHead
                                            key={day.isoDate}
                                            className={`text-center transition-colors ${isCompact ? 'p-0 px-0.5 text-[10px] min-w-0' : 'min-w-[150px]'} ${
                                                isNonWorkingDay && showHolidayHighlight ? 'bg-red-100 dark:bg-red-950' : ''
                                            } ${isWeekend && showWeekendHighlight ? 'ring-1 ring-inset ring-yellow-300/60 dark:ring-yellow-700/50 bg-yellow-50/30 dark:bg-yellow-900/10' : ''} ${isShortenedDay && showShortenedHighlight ? 'bg-orange-100 dark:bg-orange-950/50' : ''}`}
                                        >
                                            <div className="flex flex-col items-center">
                                                <span>{isCompact ? day.label.split(' ')[0] : day.label}</span>
                                                {isNonWorkingDay && !isCompact && (
                                                    <span className="text-[10px] text-red-600 dark:text-red-400 font-normal">
                                                        {holiday ? holidayInfo?.holidayName || getHolidayName(day.isoDate, orgHolidays) : 'Day Off'}
                                                    </span>
                                                )}
                                                {isShortenedDay && !isNonWorkingDay && !isCompact && (
                                                    <span className="text-[10px] text-orange-500 font-normal truncate max-w-full px-1">
                                                        {holidayInfo?.startTime}-{holidayInfo?.endTime}
                                                    </span>
                                                )}
                                            </div>
                                        </TableHead>
                                    )
                                })}
                            </TableRow>
                        </TableHeader>
                        <TableBody>
                            {rows.map((row) => (
                                <TableRow key={row.employeeId}>
                                    <TableCell
                                        className={`font-medium sticky left-0 bg-background z-10 border-r cursor-pointer select-none ${isCompact ? "px-2 py-1 text-[11px]" : ""} ${selectedEmployeeId === row.employeeId ? "bg-primary/10" : ""}`}
                                        onDoubleClick={() => handleEmployeeDoubleClick(row.employeeId)}
                                        title="Double-click to focus this employee"
                                    >
                                        <div className="flex flex-row gap-2 justify-between items-center">
                                            <div className="min-w-0">
                                                <div className="truncate">{row.employeeName}</div>
                                                {!isCompact && (
                                                    <div className="flex items-center gap-1 flex-wrap mt-0.5">
                                                        {row.departmentNames.slice(0, 2).map(name => (
                                                            <span key={name} className="text-xs text-muted-foreground">
                                                                {name}
                                                            </span>
                                                        ))}
                                                        {row.departmentNames.length > 2 && (
                                                            <Popover>
                                                                <PopoverTrigger asChild>
                                                                    <button className="text-xs text-primary underline-offset-2 hover:underline cursor-pointer">
                                                                        +{row.departmentNames.length - 2} more
                                                                    </button>
                                                                </PopoverTrigger>
                                                                <PopoverContent className="w-48 p-2" side="right">
                                                                    <p className="text-xs font-medium mb-1.5 text-muted-foreground">All departments</p>
                                                                    <ul className="space-y-1">
                                                                        {row.departmentNames.map(name => (
                                                                            <li key={name} className="text-sm">{name}</li>
                                                                        ))}
                                                                    </ul>
                                                                </PopoverContent>
                                                            </Popover>
                                                        )}
                                                    </div>
                                                )}
                                            </div>
                                            <Badge
                                                className={isCompact ? "px-1 text-[9px]" : ""}
                                                variant={
                                                    row.totalHours > 200
                                                        ? "destructive"
                                                        : row.totalHours > 180
                                                            ? "warning"
                                                            : "secondary"
                                                }
                                            >
                                                {row.totalHours.toFixed(isCompact ? 0 : 1)}h
                                            </Badge>
                                        </div>
                                    </TableCell>
                                    {daysOfMonth.map((day) => {
                                        const assignment = row.days.get(day.isoDate);
                                        const holidayInfo = orgHolidays.find((h) => {
                                            const d = new Date(day.isoDate)
                                            return h.month === d.getUTCMonth() + 1 && h.day === d.getUTCDate()
                                        })
                                        const dayDate = new Date(day.isoDate)
                                        const isWeekend = dayDate.getUTCDay() === 0 || dayDate.getUTCDay() === 6
                                        const holiday = !!holidayInfo
                                        const isShortenedDay = holidayInfo?.isShortenedDay
                                        const isNonWorkingDay = (holiday && !isShortenedDay)
                                        const timeOff = getEmployeeTimeOff(row.employeeId, day.isoDate, employeeTimeOffs)

                                        return (
                                            <TableCell
                                                key={day.isoDate}
                                                className={`transition-colors ${isCompact ? "p-0 px-0.5" : "p-2"} ${
                                                    timeOff
                                                        ? getTimeOffColor(timeOff.type)
                                                        : isNonWorkingDay && showHolidayHighlight
                                                            ? 'bg-red-50 dark:bg-red-950/10'
                                                            : isShortenedDay && showShortenedHighlight
                                                                ? 'bg-orange-50 dark:bg-orange-950/10'
                                                                : ''
                                                } ${isWeekend && showWeekendHighlight ? 'ring-1 ring-inset ring-yellow-300/60 dark:ring-yellow-700/50 bg-yellow-50/30 dark:bg-yellow-900/10' : ''}`}
                                            >
                                                {timeOff ? (
                                                    <div className="text-xs text-center py-1 px-2 rounded border">
                                                        <div className="font-medium">{t(getTimeOffLabelKey(timeOff.type))}</div>
                                                    </div>
                                                ) : (
                                                    <DroppableCell
                                                        employeeId={row.employeeId}
                                                        date={day.isoDate}
                                                        assignment={assignment}
                                                        onAssign={assignShift}
                                                        onRemove={removeShift}
                                                        onUpdateNote={updateNote}
                                                        isNonWorkingDay={isNonWorkingDay}
                                                        isCompact={isCompact}
                                                    />
                                                )}
                                            </TableCell>
                                        );
                                    })}
                                </TableRow>
                            ))}
                        </TableBody>
                    </Table>
                </div>
                )}
            </ScheduleShiftTemplates>

            <CoverageCheckModal
                open={coverageModalOpen}
                onOpenChange={setCoverageModalOpen}
                shiftTypes={shiftTypes}
                shiftsData={shiftsData}
                daysOfMonth={daysOfMonth}
                orgHolidays={orgHolidays}
                orgSchedule={orgSchedule}
                departments={departments}
            />
        </div>
    );
}
