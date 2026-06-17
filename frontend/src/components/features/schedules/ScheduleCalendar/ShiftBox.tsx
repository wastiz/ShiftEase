'use client'
import { useEffect, useRef, useState } from 'react'
import { draggable } from '@atlaskit/pragmatic-drag-and-drop/element/adapter'
import { dropTargetForElements } from '@atlaskit/pragmatic-drag-and-drop/element/adapter'
import { Trash2, UserPlus, X } from 'lucide-react'
import { toast } from 'sonner'
import { Button } from '@/components/ui/shadcn/button'
import {
    Dialog,
    DialogContent,
    DialogHeader,
    DialogTitle,
    DialogTrigger,
} from '@/components/ui/shadcn/dialog'
import NoteEditor from '@/components/features/schedules/NoteEditor'
import { Shift, EmployeeMinData } from '@/types'

function timeToMinutes(time: string): number {
    const [hours, minutes] = time.split(':').map(Number)
    return hours * 60 + minutes
}

const MARGIN = 4
const OVERLAP = 10

type ShiftBoxProps = {
    shift: Shift
    employees: EmployeeMinData[]
    shiftsData: Shift[]
    setShiftsData?: (shifts: Shift[] | ((prev: Shift[]) => Shift[])) => void
    date: string
    cellHeight?: number
    isNextDayPart?: boolean
    colIndex?: number
    colCount?: number
    isActive?: boolean
    onActivate?: () => void
}

export default function ShiftBox({
    shift,
    employees,
    shiftsData,
    setShiftsData,
    date,
    cellHeight = 40,
    isNextDayPart = false,
    colIndex = 0,
    colCount = 1,
    isActive = false,
    onActivate,
}: ShiftBoxProps) {
    const ref = useRef<HTMLDivElement>(null)
    const [isDraggingOver, setIsDraggingOver] = useState(false)
    const [isDialogOpen, setIsDialogOpen] = useState(false)

    useEffect(() => {
        if (!ref.current) return
        return draggable({
            element: ref.current,
            getInitialData: () => ({ type: 'shift', id: shift.id }),
        })
    }, [shift.id])

    useEffect(() => {
        if (!ref.current) return
        return dropTargetForElements({
            element: ref.current,
            onDrop: ({ source }) => {
                if (source.data.type === 'employee') {
                    const empId = source.data.id
                    if (shift.employees.some((e) => e.id === empId)) {
                        toast.info('Employee already assigned to this shift')
                        return
                    }

                    const alreadyAssigned = shiftsData.some(
                        (s) =>
                            s.date === date &&
                            s.id !== shift.id &&
                            s.employees.some((e) => e.id === empId)
                    )

                    if (alreadyAssigned) {
                        toast.error('Employee already has a shift on this day')
                        return
                    }

                    const emp = employees.find((e) => e.id === empId)
                    if (!emp) return

                    if (setShiftsData) {
                        setShiftsData((prev) =>
                            prev.map((s) =>
                                s.id === shift.id
                                    ? {
                                        ...s,
                                        employees: [
                                            ...s.employees,
                                            { id: emp.id, name: emp.name, note: undefined },
                                        ],
                                    }
                                    : s
                            )
                        )
                    }
                    setIsDraggingOver(false)
                }
            },
            onDragEnter: () => setIsDraggingOver(true),
            onDragLeave: () => setIsDraggingOver(false),
        })
    }, [shift, employees, date, shiftsData, setShiftsData])

    const handleRemoveShift = () => {
        if (setShiftsData) {
            setShiftsData((prev) => prev.filter((s) => s.id !== shift.id))
        }
        toast.success('Shift removed')
    }

    const handleRemoveEmployee = (empId: number) => {
        if (setShiftsData) {
            setShiftsData((prev) =>
                prev.map((s) =>
                    s.id === shift.id
                        ? {
                            ...s,
                            employees: s.employees.filter((e) => e.id !== empId),
                        }
                        : s
                )
            )
        }
    }

    const handleUpdateNote = (empId: number, note: string) => {
        if (setShiftsData) {
            setShiftsData((prev) =>
                prev.map((s) =>
                    s.id === shift.id
                        ? {
                            ...s,
                            employees: s.employees.map((e) =>
                                e.id === empId ? { ...e, note } : e
                            ),
                        }
                        : s
                )
            )
        }
        toast.success('Note updated')
    }

    const handleAddEmployee = (empId: number) => {
        const alreadyAssigned = shiftsData.some(
            (s) =>
                s.date === date &&
                s.id !== shift.id &&
                s.employees.some((e) => e.id === empId)
        )

        if (alreadyAssigned) {
            toast.error('Employee already has a shift on this day')
            return
        }

        const emp = employees.find((e) => e.id === empId)
        if (!emp) return

        if (setShiftsData) {
            setShiftsData((prev) =>
                prev.map((s) =>
                    s.id === shift.id
                        ? {
                            ...s,
                            employees: [...s.employees, { id: emp.id, name: emp.name, note: undefined }],
                        }
                        : s
                )
            )
        }
        toast.success(`${emp.name} assigned to shift`)
    }

    const startMinutes = timeToMinutes(shift.startTime)
    const endMinutes = timeToMinutes(shift.endTime)
    const isOvernight = endMinutes <= startMinutes

    let topPosition: number
    let height: number

    if (isNextDayPart) {
        topPosition = 0
        height = (endMinutes / 60) * cellHeight
    } else if (isOvernight) {
        topPosition = (startMinutes / 60) * cellHeight
        height = ((24 * 60 - startMinutes) / 60) * cellHeight
    } else {
        topPosition = (startMinutes / 60) * cellHeight
        height = ((endMinutes - startMinutes) / 60) * cellHeight
    }

    const availableEmployees = employees.filter(
        (e) => !shift.employees.some((se) => se.id === e.id)
    )

    const isLast = colIndex === colCount - 1
    const leftPercent = (colIndex / colCount) * 100
    const widthPercent = 100 / colCount

    // Non-last columns extend OVERLAP px into the next slot for the layered effect
    const widthAdjust = isLast ? -MARGIN : OVERLAP - MARGIN

    const zIndex = isActive ? 100 : colIndex + 5

    const handleClick = (e: React.MouseEvent) => {
        e.stopPropagation()
        onActivate?.()
    }

    return (
        <div
            ref={ref}
            className={`absolute rounded border-l-4 shadow-sm cursor-move transition-shadow bg-card ${
                isDraggingOver ? 'ring-2 ring-primary' : ''
            } ${isActive ? 'shadow-lg' : ''}`}
            style={{
                top: topPosition,
                height: height,
                left: `calc(${leftPercent}% + ${MARGIN}px)`,
                width: `calc(${widthPercent}% - ${MARGIN}px + ${widthAdjust}px)`,
                borderLeftColor: shift.color,
                minHeight: '60px',
                zIndex,
            }}
            onClick={handleClick}
        >
            <div className="p-2 h-full flex flex-col text-xs overflow-hidden">
                <div className="flex items-center justify-between mb-1">
                    <span className="font-semibold text-sm truncate">{shift.shiftTypeName}</span>
                    <Button
                        variant="ghost"
                        size="icon"
                        className="h-5 w-5 shrink-0"
                        onClick={(e) => { e.stopPropagation(); handleRemoveShift() }}
                    >
                        <Trash2 className="h-3 w-3" />
                    </Button>
                </div>

                <div className="text-muted-foreground mb-2">
                    {isNextDayPart ? (
                        <>
                            00:00 - {shift.endTime.slice(0, 5)}
                            <span className="ml-1 text-orange-500">(cont.)</span>
                        </>
                    ) : (
                        <>
                            {shift.startTime.slice(0, 5)} - {isOvernight ? '24:00' : shift.endTime.slice(0, 5)}
                            {isOvernight && <span className="ml-1 text-orange-500">(+1)</span>}
                        </>
                    )}
                </div>

                <div className="flex-1 space-y-1 overflow-y-auto">
                    {shift.employees.map((emp) => (
                        <div
                            key={emp.id}
                            className="bg-background/50 rounded px-2 py-1"
                        >
                            <div className="flex items-center justify-between">
                                <span className="text-xs truncate">{emp.name}</span>
                                <div className="flex items-center gap-1">
                                    <NoteEditor
                                        note={emp.note}
                                        onSave={(note) => handleUpdateNote(emp.id, note)}
                                        entityName={emp.name}
                                        buttonClassName="h-4 w-4 flex-shrink-0"
                                    />
                                    <Button
                                        variant="ghost"
                                        size="icon"
                                        className="h-4 w-4 flex-shrink-0"
                                        onClick={(e) => { e.stopPropagation(); handleRemoveEmployee(emp.id) }}
                                    >
                                        <X className="h-3 w-3" />
                                    </Button>
                                </div>
                            </div>
                            {emp.note && (
                                <div className="text-[10px] text-muted-foreground mt-1 truncate">
                                    {emp.note}
                                </div>
                            )}
                        </div>
                    ))}
                </div>

                <Dialog open={isDialogOpen} onOpenChange={setIsDialogOpen}>
                    <DialogTrigger asChild>
                        <Button
                            variant="outline"
                            size="sm"
                            className="mt-2 h-7"
                            onClick={(e) => e.stopPropagation()}
                        >
                            <UserPlus className="h-3 w-3 mr-1" />
                            Add Employee
                        </Button>
                    </DialogTrigger>
                    <DialogContent>
                        <DialogHeader>
                            <DialogTitle>Add Employee to {shift.shiftTypeName}</DialogTitle>
                        </DialogHeader>
                        <div className="space-y-2 max-h-[400px] overflow-y-auto">
                            {availableEmployees.length === 0 ? (
                                <p className="text-sm text-muted-foreground">
                                    No available employees
                                </p>
                            ) : (
                                availableEmployees.map((emp) => (
                                    <Button
                                        key={emp.id}
                                        variant="outline"
                                        className="w-full justify-start"
                                        onClick={() => {
                                            handleAddEmployee(emp.id)
                                            setIsDialogOpen(false)
                                        }}
                                    >
                                        {emp.name}
                                    </Button>
                                ))
                            )}
                        </div>
                    </DialogContent>
                </Dialog>
            </div>
        </div>
    )
}
