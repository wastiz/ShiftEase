'use client'
import { useEffect, useRef } from 'react'
import { draggable } from '@atlaskit/pragmatic-drag-and-drop/element/adapter'

type DraggableEmployeeProps = {
    employeeId: number
    employeeName: string
    departmentNames?: string[]
}

export default function DraggableEmployee({ employeeId, employeeName, departmentNames = [] }: DraggableEmployeeProps) {
    const ref = useRef<HTMLDivElement>(null)

    useEffect(() => {
        if (!ref.current) return
        return draggable({
            element: ref.current,
            getInitialData: () => ({ type: 'matrixEmployee', employeeId, employeeName }),
        })
    }, [employeeId, employeeName])

    return (
        <div
            ref={ref}
            className="flex flex-col gap-0.5 px-2 py-1.5 border rounded cursor-grab active:cursor-grabbing hover:bg-accent/50 transition-colors"
        >
            <span className="font-medium text-xs leading-tight">{employeeName}</span>
            {departmentNames.length > 0 && (
                <span className="text-[10px] text-muted-foreground truncate">
                    {departmentNames.join(', ')}
                </span>
            )}
        </div>
    )
}
