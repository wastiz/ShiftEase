'use client'
import { useEffect, useRef } from 'react'
import { draggable } from '@atlaskit/pragmatic-drag-and-drop/element/adapter'

export default function ShiftTemplateSmallCard({ name, id, startTime, endTime, color}: { name: string; id: number, startTime: string, endTime: string, color: string }) {
    const ref = useRef<HTMLDivElement>(null);

    useEffect(() => {
        if (!ref.current) return;
        return draggable({
            element: ref.current,
            getInitialData: () => ({ type: "shiftType", id: id }),
        });
    }, [id]);

    return (
        <div
            ref={ref}
            className="min-w-32 max-h-20 flex items-center gap-2 p-2 border rounded cursor-grab active:cursor-grabbing hover:bg-accent/50 transition-colors"
            style={{ borderLeftColor: color, borderLeftWidth: "4px" }}
        >
            <div className="flex-1">
                <div className="font-medium text-sm">{name}</div>
                <div className="text-xs text-muted-foreground">
                    {startTime.slice(0, 5)} - {endTime.slice(0, 5)}
                </div>
            </div>
        </div>
    );
}
