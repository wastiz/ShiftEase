'use client'
import { useEffect, useRef } from 'react'
import { draggable } from '@atlaskit/pragmatic-drag-and-drop/element/adapter'

export default function EmployeeSmallCard({
    id,
    name,
    position
}: {
    id: number;
    name: string;
    position?: string;
}) {
    const ref = useRef<HTMLDivElement>(null)

    useEffect(() => {
        if (!ref.current) return
        return draggable({
            element: ref.current,
            getInitialData: () => ({ id, type: 'employee', name }),
        })
    }, [])

    return (
        <div
            ref={ref}
            className="cursor-grab rounded border px-3 py-1 text-sm hover:bg-accent"
            data-draggable-id={`employee-${id}`}
        >
            <div className="font-medium">{name}</div>
            {position && (
                <div className="text-xs text-muted-foreground">{position}</div>
            )}
        </div>
    )
}
