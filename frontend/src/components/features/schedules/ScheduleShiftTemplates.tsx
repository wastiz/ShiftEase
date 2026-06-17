'use client'
import ShiftTemplateSmallCard from "@/components/ui/cards/ShiftTypeSmallCard"
import { ShiftTemplate } from "@/types"
import { ReactNode } from "react"

interface ScheduleShiftTemplatesProps {
    isEditable?: boolean
    layoutPosition?: 'left' | 'top'
    shiftTypes: ShiftTemplate[]
    children: ReactNode
}

export default function ScheduleShiftTemplates({
    isEditable = true,
    layoutPosition = 'left',
    shiftTypes,
    children,
}: ScheduleShiftTemplatesProps) {
    const showTop = isEditable && layoutPosition === 'top'
    const showLeft = isEditable && layoutPosition === 'left'

    return (
        <div className="flex flex-col flex-1 min-h-0 gap-4">
            {showTop && (
                <div className="border rounded-lg bg-muted/50 p-4 shrink-0">
                    <h4 className="font-semibold mb-2 text-sm">Shift Types</h4>
                    <div className="flex gap-2 overflow-x-auto pb-1">
                        {shiftTypes.map(st => (
                            <ShiftTemplateSmallCard
                                key={st.id}
                                id={st.id}
                                name={st.name}
                                startTime={st.startTime}
                                endTime={st.endTime}
                                color={st.color}
                            />
                        ))}
                    </div>
                </div>
            )}

            <div className="flex flex-1 min-h-0 gap-4">
                {showLeft && (
                    <aside className="border rounded-lg bg-muted/50 w-56 shrink-0 p-4 overflow-y-auto">
                        <h4 className="font-semibold mb-2 text-sm">Shift Types</h4>
                        <div className="space-y-2">
                            {shiftTypes.map(st => (
                                <ShiftTemplateSmallCard
                                    key={st.id}
                                    id={st.id}
                                    name={st.name}
                                    startTime={st.startTime}
                                    endTime={st.endTime}
                                    color={st.color}
                                />
                            ))}
                        </div>
                    </aside>
                )}
                {children}
            </div>
        </div>
    )
}
