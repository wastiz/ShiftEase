'use client'
import { useEffect, useRef, useState, useMemo } from 'react'
import { dropTargetForElements } from '@atlaskit/pragmatic-drag-and-drop/element/adapter'
import { toast } from 'sonner'
import ShiftBox from './ShiftBox'
import { Holiday, WorkDay, Shift, ShiftTemplate, EmployeeMinData } from "@/types";
import { EmployeeTimeOff, TimeOffType } from "@/types/schedule";
import {
    getEmployeeTimeOff,
    getPreviousDay, getTimeOffColor,
    getTimeOffLabelKey,
    isHoliday,
    isWorkingDay,
    timeToMinutes
} from "@/helpers/dateHelper";

const CELL_HEIGHT = 40
const HOURS = Array.from({ length: 24 }, (_, i) => `${i.toString().padStart(2, '0')}:00`)

type ShiftLayout = { colIndex: number; colCount: number }

function computeOverlapLayouts(
    shiftsInDay: Shift[],
    previousDayShifts: Shift[],
): Map<string, ShiftLayout> {
    type Item = { key: string; start: number; end: number }

    const items: Item[] = [
        ...shiftsInDay.map(s => {
            const startM = timeToMinutes(s.startTime)
            const endM = timeToMinutes(s.endTime)
            const overnight = endM <= startM
            return { key: String(s.id), start: startM, end: overnight ? 24 * 60 : endM }
        }),
        ...previousDayShifts.map(s => {
            const endM = timeToMinutes(s.endTime)
            return { key: `${s.id}-next`, start: 0, end: endM }
        }),
    ]

    const n = items.length
    if (n === 0) return new Map()

    const sortedIdx = Array.from({ length: n }, (_, i) => i)
        .sort((a, b) => items[a].start - items[b].start)

    const colEnds: number[] = []
    const colOf: number[] = new Array(n)

    for (const i of sortedIdx) {
        const { start, end } = items[i]
        let col = -1
        for (let c = 0; c < colEnds.length; c++) {
            if (colEnds[c] <= start) { col = c; colEnds[c] = end; break }
        }
        if (col === -1) { col = colEnds.length; colEnds.push(end) }
        colOf[i] = col
    }

    // Union-Find to group overlapping shifts
    const parent = Array.from({ length: n }, (_, i) => i)
    function find(x: number): number {
        if (parent[x] !== x) parent[x] = find(parent[x])
        return parent[x]
    }
    for (let i = 0; i < n; i++) {
        for (let j = i + 1; j < n; j++) {
            if (items[i].start < items[j].end && items[i].end > items[j].start) {
                parent[find(i)] = find(j)
            }
        }
    }

    const compMaxCol = new Map<number, number>()
    for (let i = 0; i < n; i++) {
        const r = find(i)
        compMaxCol.set(r, Math.max(compMaxCol.get(r) ?? 0, colOf[i] + 1))
    }

    const result = new Map<string, ShiftLayout>()
    for (let i = 0; i < n; i++) {
        result.set(items[i].key, {
            colIndex: colOf[i],
            colCount: compMaxCol.get(find(i)) ?? 1,
        })
    }
    return result
}

type DayContainerProps = {
    date: string
    dateLabel: string
    shiftTypes: ShiftTemplate[]
    employees: EmployeeMinData[]
    shiftsData: Shift[]
    setShiftsData?: (shifts: Shift[] | ((prev: Shift[]) => Shift[])) => void
    isEditable?: boolean
    cellHeight?: number
    holidays?: Holiday[]
    workDays?: WorkDay[]
    employeeTimeOffs?: EmployeeTimeOff[]
    showWeekendHighlight?: boolean
    showHolidayHighlight?: boolean
    showShortenedHighlight?: boolean
}

export default function DayContainer({
    date,
    dateLabel,
    shiftTypes,
    employees,
    shiftsData,
    setShiftsData,
    isEditable = true,
    cellHeight = 40,
    holidays = [],
    workDays = [],
    employeeTimeOffs = [],
    showWeekendHighlight = true,
    showHolidayHighlight = true,
    showShortenedHighlight = true,
}: DayContainerProps) {
    const ref = useRef<HTMLDivElement>(null)
    const [isDraggingOver, setIsDraggingOver] = useState(false)
    const [activeShiftKey, setActiveShiftKey] = useState<string | null>(null)

    const shiftsInDay = shiftsData.filter((s) => s.date === date)

    const previousDay = getPreviousDay(date);
    const previousDayShifts = shiftsData.filter((s) => {
        if (s.date !== previousDay) return false;
        const startMinutes = timeToMinutes(s.startTime);
        const endMinutes = timeToMinutes(s.endTime);
        return endMinutes <= startMinutes;
    });

    const overlapLayouts = useMemo(
        () => computeOverlapLayouts(shiftsInDay, previousDayShifts),
        // eslint-disable-next-line react-hooks/exhaustive-deps
        [shiftsInDay.map(s => s.id).join(','), previousDayShifts.map(s => s.id).join(',')]
    )

    const holidayInfo = holidays.find((h) => {
        const d = new Date(date)
        return h.month === d.getUTCMonth() + 1 && h.day === d.getUTCDate()
    })

    const holiday = !!holidayInfo
    const working = isWorkingDay(date, workDays)
    const isShortenedDay = holidayInfo?.isShortenedDay
    const isNonWorkingDay = (holiday && !isShortenedDay) || (!working && !isShortenedDay)

    const dayDate = new Date(date)
    const isWeekend = dayDate.getUTCDay() === 0 || dayDate.getUTCDay() === 6

    useEffect(() => {
        if (!isEditable) return
        if (!ref.current) return
        return dropTargetForElements({
            element: ref.current,
            onDrop: ({ source }) => {
                if (source.data.type === 'shiftTemplate') {
                    const st = shiftTypes.find((x) => x.id === source.data.id)
                    if (!st) return

                    if (shiftsInDay.some((s) => s.shiftTypeId === st.id)) {
                        toast.error('This shift type already exists on this day')
                        return
                    }

                    let sTime = st.startTime
                    let eTime = st.endTime

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

                    if (setShiftsData) {
                        setShiftsData((prev) => [
                            ...prev,
                            {
                                id: Date.now(),
                                shiftTypeName: st.name,
                                shiftTypeId: st.id,
                                startTime: sTime,
                                endTime: eTime,
                                date,
                                color: st.color,
                                employeeNeeded: 1,
                                employees: [],
                            },
                        ])
                    }
                    setIsDraggingOver(false)
                }

                if (source.data.type === 'shift') {
                    const shiftId = source.data.id
                    const shift = shiftsData.find((s) => s.id === shiftId)
                    if (!shift) return

                    if (shift.date === date) {
                        toast.info('Shift is already on this day')
                        return
                    }

                    if (shiftsInDay.some((s) => s.shiftTypeId === shift.shiftTypeId)) {
                        toast.error('This shift type already exists on this day')
                        return
                    }

                    if (setShiftsData) {
                        setShiftsData((prev) =>
                            prev.map((s) =>
                                s.id === shiftId ? { ...s, date } : s
                            )
                        )
                    }
                    setIsDraggingOver(false)
                }
            },
            onDragEnter: () => setIsDraggingOver(true),
            onDragLeave: () => setIsDraggingOver(false),
        })
    }, [shiftTypes, shiftsData, shiftsInDay, date, setShiftsData, isEditable])

    const totalHeight = HOURS.length * CELL_HEIGHT

    const timeOffsForDate = employees
        .map(emp => {
            const timeOff = getEmployeeTimeOff(emp.id, date, employeeTimeOffs)
            return timeOff
                ? { ...timeOff, employeeName: emp.name }
                : null
        })
        .filter(Boolean) as (EmployeeTimeOff & { employeeName: string })[]

    const headerBg = (isWeekend && showWeekendHighlight)
        ? 'bg-yellow-50/40 dark:bg-yellow-900/10'
        : ''

    return (
        <div ref={ref} className={`border-l relative ${headerBg}`} onClick={() => setActiveShiftKey(null)}>
            <div className="h-10 flex flex-col items-center justify-center border-b font-medium">
                <div>{dateLabel}</div>

                {timeOffsForDate.length > 0 && (
                    <div className="flex gap-0.5 mt-0.5">
                        {timeOffsForDate.map((timeOff, idx) => (
                            <div
                                key={idx}
                                className={`w-4 h-4 rounded-sm text-[10px] font-bold flex items-center justify-center
                        ${getTimeOffColor(timeOff.type)}`}
                                title={`${timeOff.employeeName}`}
                            >
                                {getTimeOffLabelKey(timeOff.type).charAt(0)}
                            </div>
                        ))}
                    </div>
                )}
            </div>

            <div className="relative" style={{ height: totalHeight }}>
                {isNonWorkingDay && showHolidayHighlight && (
                    <>
                        <div className="absolute inset-0 bg-stripes opacity-10 pointer-events-none z-[1]" />
                        <div
                            className="absolute top-1/2 left-1/2 transform -translate-x-1/2 -translate-y-1/2 rotate-[-45deg] text-red-500 font-bold text-xl opacity-20 pointer-events-none z-[2] whitespace-nowrap">
                            {holiday ? holidayInfo?.holidayName || 'Holiday' : 'Day Off'}
                        </div>
                    </>
                )}

                {isShortenedDay && !isNonWorkingDay && showShortenedHighlight && (
                    <>
                        <div className="absolute inset-0 bg-stripes opacity-5 pointer-events-none z-[1]" />
                        <div
                            className="absolute top-1/4 left-1/2 transform -translate-x-1/2 -translate-y-1/2 text-orange-500 font-bold text-md opacity-40 pointer-events-none z-[2] whitespace-nowrap">
                            {holidayInfo?.holidayName} (Shortened: {holidayInfo?.startTime} - {holidayInfo?.endTime})
                        </div>
                    </>
                )}

                {HOURS.map((_, index) => (
                    <div
                        key={index}
                        className="absolute w-full border-b"
                        style={{
                            top: index * CELL_HEIGHT,
                            height: CELL_HEIGHT
                        }}
                    />
                ))}

                {shiftsInDay.map((shift) => {
                    const layout = overlapLayouts.get(String(shift.id)) ?? { colIndex: 0, colCount: 1 }
                    return (
                        <ShiftBox
                            key={shift.id}
                            shift={shift}
                            employees={employees}
                            shiftsData={shiftsData}
                            setShiftsData={setShiftsData}
                            date={date}
                            cellHeight={CELL_HEIGHT}
                            colIndex={layout.colIndex}
                            colCount={layout.colCount}
                            isActive={activeShiftKey === String(shift.id)}
                            onActivate={() => setActiveShiftKey(String(shift.id))}
                        />
                    )
                })}

                {previousDayShifts.map((shift) => {
                    const layout = overlapLayouts.get(`${shift.id}-next`) ?? { colIndex: 0, colCount: 1 }
                    return (
                        <ShiftBox
                            key={`${shift.id}-nextday`}
                            shift={shift}
                            employees={employees}
                            shiftsData={shiftsData}
                            setShiftsData={setShiftsData}
                            date={previousDay}
                            cellHeight={CELL_HEIGHT}
                            isNextDayPart={true}
                            colIndex={layout.colIndex}
                            colCount={layout.colCount}
                            isActive={activeShiftKey === `${shift.id}-next`}
                            onActivate={() => setActiveShiftKey(`${shift.id}-next`)}
                        />
                    )
                })}
            </div>
        </div>
    )
}
