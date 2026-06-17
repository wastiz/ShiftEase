import { useEffect, useRef, useState } from 'react'
import { draggable, dropTargetForElements } from '@atlaskit/pragmatic-drag-and-drop/element/adapter'
import { X } from 'lucide-react'
import { EmployeeShiftAssignment } from '@/types/schedule'

type MatrixCellProps = {
    date: string
    shiftTypeId: number
    shiftColor: string
    employees: EmployeeShiftAssignment[]
    minEmployees: number
    maxEmployees: number
    isNonWorkingDay?: boolean
    isWeekend?: boolean
    showWeekendHighlight?: boolean
    onAssign: (employeeId: number, employeeName: string, date: string, shiftTypeId: number) => void
    onMove: (employeeId: number, fromDate: string, fromShiftTypeId: number, toDate: string, toShiftTypeId: number) => void
    onRemove: (employeeId: number, date: string, shiftTypeId: number) => void
}

function DraggableEmployeeBadge({
    assignment,
    date,
    shiftTypeId,
    color,
    onRemove,
}: {
    assignment: EmployeeShiftAssignment
    date: string
    shiftTypeId: number
    color: string
    onRemove: (employeeId: number, date: string, shiftTypeId: number) => void
}) {
    const ref = useRef<HTMLDivElement>(null)

    useEffect(() => {
        if (!ref.current) return
        return draggable({
            element: ref.current,
            getInitialData: () => ({
                type: 'matrixAssignment',
                employeeId: assignment.id,
                employeeName: assignment.name,
                fromDate: date,
                fromShiftTypeId: shiftTypeId,
            }),
        })
    }, [assignment.id, date, shiftTypeId])

    return (
        <div
            ref={ref}
            className="flex items-center gap-1.5 px-2 py-1 rounded text-xs font-medium cursor-grab active:cursor-grabbing group"
            style={{ backgroundColor: `${color}20`, border: `1px solid ${color}50` }}
        >
            <span className="truncate">{assignment.name}</span>
            <button
                onPointerDown={(e) => e.stopPropagation()}
                onClick={(e) => {
                    e.stopPropagation()
                    onRemove(assignment.id, date, shiftTypeId)
                }}
                className="opacity-0 group-hover:opacity-100 transition-opacity shrink-0"
            >
                <X className="h-3 w-3" />
            </button>
        </div>
    )
}

export default function MatrixCell({
    date,
    shiftTypeId,
    shiftColor,
    employees,
    minEmployees,
    isNonWorkingDay = false,
    isWeekend = false,
    showWeekendHighlight = true,
    onAssign,
    onMove,
    onRemove,
}: MatrixCellProps) {
    const ref = useRef<HTMLDivElement>(null)
    const [isDraggingOver, setIsDraggingOver] = useState(false)

    useEffect(() => {
        if (!ref.current) return
        return dropTargetForElements({
            element: ref.current,
            onDrop: ({ source }) => {
                if (source.data.type === 'matrixEmployee') {
                    onAssign(
                        source.data.employeeId as number,
                        source.data.employeeName as string,
                        date,
                        shiftTypeId,
                    )
                } else if (source.data.type === 'matrixAssignment') {
                    onMove(
                        source.data.employeeId as number,
                        source.data.fromDate as string,
                        source.data.fromShiftTypeId as number,
                        date,
                        shiftTypeId,
                    )
                }
                setIsDraggingOver(false)
            },
            onDragEnter: () => setIsDraggingOver(true),
            onDragLeave: () => setIsDraggingOver(false),
        })
    }, [date, shiftTypeId, onAssign, onMove])

    const count = employees.length

    let bgClass = ''
    if (isNonWorkingDay) {
        bgClass = 'bg-red-50 dark:bg-red-950/20'
    } else if (count === 0 && minEmployees > 0) {
        bgClass = 'bg-red-50/60 dark:bg-red-950/10'
    } else if (count > 0 && count < minEmployees) {
        bgClass = 'bg-amber-50 dark:bg-amber-950/20'
    } else if (count >= minEmployees && minEmployees > 0) {
        bgClass = 'bg-green-50/50 dark:bg-green-950/10'
    }

    const weekendClass = isWeekend && showWeekendHighlight && !isNonWorkingDay
        ? 'ring-1 ring-inset ring-yellow-300/60 dark:ring-yellow-700/50'
        : ''

    return (
        <div
            ref={ref}
            className={`min-h-[90px] p-1.5 flex flex-col gap-1 transition-colors ${bgClass} ${weekendClass} ${
                isDraggingOver ? 'ring-2 ring-primary ring-inset' : ''
            }`}
        >
            {employees.map(emp => (
                <DraggableEmployeeBadge
                    key={emp.id}
                    assignment={emp}
                    date={date}
                    shiftTypeId={shiftTypeId}
                    color={shiftColor}
                    onRemove={onRemove}
                />
            ))}
            {!isNonWorkingDay && count < minEmployees && (
                <div className="flex-1 flex items-center justify-center text-[10px] text-muted-foreground/60 mt-0.5">
                    {count}/{minEmployees}
                </div>
            )}
        </div>
    )
}
