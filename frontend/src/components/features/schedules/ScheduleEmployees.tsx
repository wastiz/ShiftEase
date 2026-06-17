'use client'
import EmployeeSmallCard from "@/components/ui/cards/EmployeeSmallCard"
import { EmployeeMinData } from "@/types"
import { ReactNode } from "react"

interface ScheduleEmployeesProps {
    isEditable?: boolean
    layoutPosition?: 'left' | 'top'
    employees: EmployeeMinData[]
    children: ReactNode
    /** Override the default EmployeeSmallCard list (e.g. DraggableEmployee for Matrix) */
    sidebarChildren?: ReactNode
}

export default function ScheduleEmployees({
    isEditable = true,
    layoutPosition = 'left',
    employees,
    children,
    sidebarChildren,
}: ScheduleEmployeesProps) {
    const showTop = isEditable && layoutPosition === 'top'
    const showLeft = isEditable && layoutPosition === 'left'

    const defaultList = (horizontal: boolean) =>
        horizontal ? (
            <div className="flex gap-2 overflow-x-auto pb-1">
                {employees.map(emp => (
                    <EmployeeSmallCard key={emp.id} id={emp.id} name={emp.name} position={emp.position} />
                ))}
            </div>
        ) : (
            <div className="space-y-2">
                {employees.map(emp => (
                    <EmployeeSmallCard key={emp.id} id={emp.id} name={emp.name} position={emp.position} />
                ))}
            </div>
        )

    return (
        <div className="flex flex-col flex-1 min-h-0 gap-4">
            {showTop && (
                <div className="border rounded-lg bg-muted/50 p-4 shrink-0">
                    <h4 className="font-semibold mb-2 text-sm">Employees</h4>
                    {sidebarChildren ?? defaultList(true)}
                </div>
            )}

            <div className="flex flex-1 min-h-0 gap-4">
                {showLeft && (
                    <aside className="border rounded-lg bg-muted/50 w-56 shrink-0 p-4 overflow-y-auto">
                        <h4 className="font-semibold mb-2 text-sm">Employees</h4>
                        {sidebarChildren ?? defaultList(false)}
                    </aside>
                )}
                {children}
            </div>
        </div>
    )
}
